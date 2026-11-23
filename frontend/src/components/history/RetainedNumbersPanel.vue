<template>
  <div class="history-panel panel">
    <h3 class="panel-title" @click="collapsed = !collapsed"><span class="toggle-arrow">{{ collapsed ? '▶' : '▼' }}</span> 保留号码 (本期)</h3>
    <div v-show="!collapsed">
      <div v-if="!issueId" class="hint">请先选择期号</div>
      <div v-else-if="retainedList.length === 0" class="hint">暂无保留号码</div>
      <div v-else class="history-list">
        <div v-for="item in retainedList" :key="item.id" class="history-item">
          <div class="history-numbers-row">
            <NumberSquareGroup
              :frontNumbers="item.frontNumbers.map(n => String(n).padStart(2,'0'))"
              :backNumbers="item.backNumbers.map(n => String(n).padStart(2,'0'))"
              :frontLabel="lbl.front" :backLabel="lbl.back"
              :frontHighlights="hl(item.frontNumbers).front"
              :backHighlights="hl(item.backNumbers).back"
              :editable="false"
              :small="true"
            />
          </div>
          <div class="history-meta-row"><span class="history-time">{{ formatTime(item.createdAt) }}</span><div class="history-actions"><button class="btn btn-sm btn-primary" @click="restore(item)">回填</button><button class="btn btn-sm btn-danger" @click="remove(item.id)">删除</button></div></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import NumberSquareGroup from '../common/NumberSquareGroup.vue'
import { ns, gameLabels } from '../../utils/gameHelper'

export default {
  name: 'RetainedNumbersPanel',
  components: { NumberSquareGroup },
  inject: ['game'],
  props: { highlightData: { type: Object, default: null } },
  data() { return { collapsed: false } },
  computed: {
    lbl() { return gameLabels(this.game) },
    issueId() { return this.$store.getters[ns(this.game, 'issue/currentIssueId')] },
    retainedList() { return this.game === 'ssq' ? this.$store.state.ssq.ssqRetained.retainedList : this.$store.state.retainedNumbers.retainedList },
    inputFrontSet() {
      if (!this.highlightData) return new Set()
      return new Set(this.highlightData.frontNumbers || [])
    },
    inputBackSet() {
      if (!this.highlightData) return new Set()
      return new Set(this.highlightData.backNumbers || [])
    }
  },
  watch: { issueId(val) { if (val) this.fetch() } },
  methods: {
    dispatch(p, d) { return this.$store.dispatch(ns(this.game, p), d) },
    commit(p, d) { this.$store.commit(ns(this.game, p), d) },
    fetch() { return this.dispatch('retainedNumbers/fetchRetained', this.issueId) },
    hl(numbers) {
      const isFront = numbers.length > 2
      const inputSet = isFront ? this.inputFrontSet : this.inputBackSet
      const result = {}
      for (const n of numbers) {
        if (inputSet.has(n)) {
          result[String(n).padStart(2, '0')] = [0]
        }
      }
      return isFront ? { front: result, back: {} } : { front: {}, back: result }
    },
    formatTime(d) { return d ? new Date(d).toLocaleString('zh-CN') : '' },
    restore(item) { this.commit('generator/SET_CURRENT_FRONT', item.frontNumbers.map(n => String(n).padStart(2, '0'))); this.commit('generator/SET_CURRENT_BACK', item.backNumbers.map(n => String(n).padStart(2, '0'))) },
    async remove(id) { if (!confirm('确定删除？')) return; await this.dispatch('retainedNumbers/deleteRetained', { id, issueId: this.issueId }) }
  }
}
</script>
