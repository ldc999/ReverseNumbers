<template>
  <div class="issue-manager">
    <h3>期号管理</h3>
    <div class="manager-add">
      <input v-model="newIssueNumber" class="input" placeholder="输入期号，如 2024001" @keyup.enter="create" />
      <button class="btn btn-primary" @click="create">新增</button>
    </div>
    <div class="manager-list">
      <div v-for="issue in issueList" :key="issue.id" class="manager-item-card">
        <div class="manager-item-row">
          <input v-if="editingId === issue.id" v-model="editNumber" class="input" @keyup.enter="update(issue.id)" />
          <strong v-else>{{ issue.issueNumber }}</strong>
          <div class="item-actions">
            <template v-if="editingId === issue.id">
              <button class="btn btn-sm btn-primary" @click="update(issue.id)">保存</button>
              <button class="btn btn-sm" @click="editingId = null">取消</button>
            </template>
            <template v-else>
              <button class="btn btn-sm" @click="startEdit(issue)">编辑</button>
              <button class="btn btn-sm btn-danger" @click="remove(issue.id)">删除</button>
            </template>
          </div>
        </div>
        <!-- Draw result row -->
        <div class="draw-result-row">
          <label class="draw-label">开奖{{ lbl.front }}:</label>
          <input v-model="drawInputs[issue.id].front" class="input input-narrow-wide" :placeholder="'如: 01,05,12'" />
          <label class="draw-label">{{ lbl.back }}:</label>
          <input v-model="drawInputs[issue.id].back" class="input input-narrow-wide" :placeholder="'如: 03'" />
          <button class="btn btn-sm" @click="saveDraw(issue)">保存开奖</button>
          <span v-if="drawSaved[issue.id]" class="draw-saved-ok">✓ 已保存</span>
        </div>
      </div>
    </div>
    <button class="btn" @click="$emit('close')">关闭</button>
  </div>
</template>

<script>
import { ns, gameLabels } from '../../utils/gameHelper'
import issueApi from '../../api/issue'
import { ssqIssueApi } from '../../api/ssq'

export default {
  name: 'IssueManager',
  inject: ['game'],
  data() {
    return {
      newIssueNumber: '', editingId: null, editNumber: '',
      drawInputs: {}, drawSaved: {}
    }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    issueList() { return this.game === 'ssq' ? this.$store.state.ssq.ssqIssue.issueList : this.$store.state.issue.issueList }
  },
  watch: {
    issueList: {
      handler(list) {
        // Initialize draw inputs from existing data
        for (const issue of list) {
          if (!(issue.id in this.drawInputs)) {
            const f = issue.winningFrontNumbers || issue.winningRedNumbers || ''
            const b = issue.winningBackNumbers || issue.winningBlueNumbers || ''
            this.$set(this.drawInputs, issue.id, { front: f, back: b })
          }
        }
      },
      immediate: true
    }
  },
  methods: {
    dispatch(p, d) { return this.$store.dispatch(ns(this.game, p), d) },
    fetch() { return this.dispatch('issue/fetchIssues') },
    async create() { if (!this.newIssueNumber.trim()) return; await this.dispatch('issue/createIssue', this.newIssueNumber.trim()); this.newIssueNumber = '' },
    startEdit(i) { this.editingId = i.id; this.editNumber = i.issueNumber },
    async update(id) { if (!this.editNumber.trim()) return; await this.dispatch('issue/updateIssue', { id, issueNumber: this.editNumber.trim() }); this.editingId = null },
    async remove(id) { if (confirm('确定删除？')) await this.dispatch('issue/deleteIssue', id) },
    async saveDraw(issue) {
      const inp = this.drawInputs[issue.id]
      if (!inp) return
      const api = this.game === 'ssq' ? ssqIssueApi : issueApi
      await api.setDrawResult(issue.id, inp.front || '', inp.back || '')
      this.$set(this.drawSaved, issue.id, true)
      setTimeout(() => this.$set(this.drawSaved, issue.id, false), 2000)
      await this.fetch()
    }
  },
  mounted() { this.fetch() }
}
</script>
