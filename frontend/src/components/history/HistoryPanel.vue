<template>
  <div class="history-panel panel">
    <h3 class="panel-title" @click="collapsed = !collapsed"><span class="toggle-arrow">{{ collapsed ? '▶' : '▼' }}</span> 选号历史</h3>
    <div v-show="!collapsed">
      <div v-if="!issueId" class="hint">请先选择期号</div>
      <div v-else-if="historyList.length === 0" class="hint">暂无历史记录</div>
      <div v-else class="history-list">
        <HistoryItem v-for="item in historyList" :key="item.id" :item="item" :highlightData="highlightData" @restore="restoreNumbers" @delete="deleteItem" />
      </div>
    </div>
    <HistoryStatsPanel :historyList="historyList" :highlightData="highlightData" />
  </div>
</template>

<script>
import HistoryItem from './HistoryItem.vue'
import HistoryStatsPanel from './HistoryStatsPanel.vue'
import { ns } from '../../utils/gameHelper'

export default {
  name: 'HistoryPanel',
  components: { HistoryItem, HistoryStatsPanel },
  inject: ['game'],
  props: { highlightData: { type: Object, default: null } },
  data() { return { collapsed: false } },
  computed: {
    issueId() { return this.$store.getters[ns(this.game, 'issue/currentIssueId')] },
    historyList() { return this.game === 'ssq' ? this.$store.state.ssq.ssqHistory.historyList : this.$store.state.history.historyList }
  },
  watch: { issueId(val) { if (val) this.fetch() } },
  methods: {
    dispatch(p, d) { return this.$store.dispatch(ns(this.game, p), d) },
    commit(p, d) { this.$store.commit(ns(this.game, p), d) },
    fetch() { return this.dispatch('history/fetchHistory', this.issueId) },
    restoreNumbers(item) {
      this.commit('generator/SET_CURRENT_FRONT', item.frontNumbers.map(n => String(n).padStart(2, '0')))
      this.commit('generator/SET_CURRENT_BACK', item.backNumbers.map(n => String(n).padStart(2, '0')))
      if (item.matchDetails) {
        try {
          const d = typeof item.matchDetails === 'string' ? JSON.parse(item.matchDetails) : item.matchDetails
          this.commit('generator/SET_HIGHLIGHT_RESULT', { frontNumbers: item.frontNumbers, backNumbers: item.backNumbers, frontMatchDetails: d.front || {}, backMatchDetails: d.back || {}, iterationCount: 0 })
        } catch (e) { /* ignore */ }
      }
    },
    async deleteItem(id) { if (!confirm('确定删除？')) return; await this.dispatch('history/deleteHistory', { id, issueId: this.issueId }) }
  }
}
</script>
