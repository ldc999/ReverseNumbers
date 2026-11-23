<template>
  <div class="history-item">
    <div class="history-numbers-row">
    
      <NumberSquareGroup
        :frontNumbers="frontNums" :backNumbers="backNums"
        :frontHighlights="frontHighlights" :backHighlights="backHighlights"
        :editable="false" :small="true"
        :frontLabel="lbl.front" :backLabel="lbl.back"
      />
    </div>
    <div class="history-meta-row">
      <span class="history-time">{{ formatTime(item.createdAt) }}</span>
        <span class="history-type" :class="{ generated: item.isGenerated, manual: !item.isGenerated }">
        {{ item.isGenerated ? '随机' + (item.iterationCount ? ' ('+item.iterationCount+'次)' : '') : '手动' }}
      </span>
      <div class="history-actions">
        <button class="btn btn-sm btn-primary" @click="$emit('restore', item)">回填</button>
        <button class="btn btn-sm btn-danger" @click="$emit('delete', item.id)">删除</button>
      </div>
    </div>
  </div>
</template>

<script>
import NumberSquareGroup from '../common/NumberSquareGroup.vue'
import { gameLabels } from '../../utils/gameHelper'

export default {
  name: 'HistoryItem',
  components: { NumberSquareGroup },
  inject: ['game'],
  props: {
    item: { type: Object, required: true },
    highlightData: { type: Object, default: null }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    frontNums() { return this.item.frontNumbers.map(n => String(n).padStart(2, '0')) },
    backNums() { return this.item.backNumbers.map(n => String(n).padStart(2, '0')) },
    frontHighlights() {
      if (!this.highlightData) return {}
      const inputSet = new Set(this.highlightData.frontNumbers || [])
      const result = {}
      for (const n of this.item.frontNumbers) {
        const key = String(n).padStart(2, '0')
        if (inputSet.has(n)) result[key] = [0]
      }
      return result
    },
    backHighlights() {
      if (!this.highlightData) return {}
      const inputSet = new Set(this.highlightData.backNumbers || [])
      const result = {}
      for (const n of this.item.backNumbers) {
        const key = String(n).padStart(2, '0')
        if (inputSet.has(n)) result[key] = [0]
      }
      return result
    }
  },
  methods: {
    formatTime(dateStr) {
      if (!dateStr) return ''
      return new Date(dateStr).toLocaleString('zh-CN')
    }
  }
}
</script>
