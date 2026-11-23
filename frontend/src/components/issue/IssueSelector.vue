<template>
  <div class="issue-selector">
    <div class="selector-row">
      <select v-model="selectedIssueId" class="issue-select" @change="onSelectIssue">
        <option value="">-- 请选择期号 --</option>
        <option v-for="issue in issueList" :key="issue.id" :value="issue.id">{{ issue.issueNumber }}</option>
      </select>
      <button class="btn btn-sm" @click="showManager = true">管理</button>
    </div>
    <!-- Draw result for current issue -->
    <div v-if="currentIssue && drawInfo && !blindMode" class="draw-display">
      <span class="draw-badge">开奖</span>
      <span>{{ lbl.front }}: <strong>{{ drawInfo.front }}</strong></span>
      <span>{{ lbl.back }}: <strong>{{ drawInfo.back }}</strong></span>
      <button class="btn btn-sm btn-primary" @click="fillDraw">回填</button>
    </div>
    <div v-if="showManager" class="modal-overlay" @click.self="showManager = false">
      <div class="modal"><IssueManager @close="showManager = false" /></div>
    </div>
  </div>
</template>

<script>
import IssueManager from './IssueManager.vue'
import { ns, gameLabels } from '../../utils/gameHelper'

export default {
  name: 'IssueSelector',
  components: { IssueManager },
  inject: ['game'],
  data() { return { showManager: false, selectedIssueId: '' } },
  computed: {
    lbl() { return gameLabels(this.game) },
    issueList() { return this.game === 'ssq' ? this.$store.state.ssq.ssqIssue.issueList : this.$store.state.issue.issueList },
    currentIssue() { return this.game === 'ssq' ? this.$store.state.ssq.ssqIssue.currentIssue : this.$store.state.issue.currentIssue },
    blindMode() { return this.game === 'ssq' ? this.$store.state.ssq.ssqGenerator.blindMode : this.$store.state.generator.blindMode },
    drawInfo() {
      if (this.blindMode || !this.currentIssue) return null
      const f = this.currentIssue.winningFrontNumbers || this.currentIssue.winningRedNumbers
      const b = this.currentIssue.winningBackNumbers || this.currentIssue.winningBlueNumbers
      if (!f && !b) return null
      return { front: f || '--', back: b || '--' }
    }
  },
  watch: {
    currentIssue(val) { this.selectedIssueId = val ? val.id : '' }
  },
  methods: {
    async fetch() { await this.$store.dispatch(ns(this.game, 'issue/fetchIssues')) },
    fillDraw() {
      if (!this.drawInfo) return
      const f = this.drawInfo.front.split(',').map(s => s.trim())
      const b = this.drawInfo.back.split(',').map(s => s.trim())
      this.$store.commit(ns(this.game, 'generator/SET_CURRENT_FRONT'), f)
      this.$store.commit(ns(this.game, 'generator/SET_CURRENT_BACK'), b)
    },
    onSelectIssue() {
      const issue = this.issueList.find(i => i.id === this.selectedIssueId)
      if (issue) this.$store.dispatch(ns(this.game, 'issue/selectIssue'), issue)
    }
  },
  mounted() { this.fetch() }
}
</script>
