<template>
  <div class="player-set-row">
    <span class="set-index">{{ index + 1 }}</span>
    <NumberSquareGroup
      :frontNumbers="frontNums" :backNumbers="backNums"
      :frontHighlights="frontHighlights" :backHighlights="backHighlights"
      :editable="false"
      :frontLabel="lbl.front" :backLabel="lbl.back"
    />
    <button class="btn btn-danger btn-sm" @click="$emit('delete')">删除</button>
  </div>
</template>

<script>
import NumberSquareGroup from '../common/NumberSquareGroup.vue'
import { gameLabels } from '../../utils/gameHelper'

function buildHighlights(setNumbers, inputNumbers) {
  if (!inputNumbers || inputNumbers.length === 0) return {}
  const inputSet = new Set(inputNumbers)
  const result = {}
  for (const s of setNumbers) {
    const n = parseInt(s)
    if (inputSet.has(n)) {
      result[String(n).padStart(2, '0')] = [0]
    }
  }
  return result
}

export default {
  name: 'PlayerNumberSet',
  components: { NumberSquareGroup },
  inject: ['game'],
  props: {
    set: { type: Object, required: true },
    index: { type: Number, default: 0 },
    highlightData: { type: Object, default: null }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    frontNums() { return this.set.frontNumbers ? this.set.frontNumbers.split(',').map(s => s.trim()) : (this.set.redNumbers || '').split(',').map(s => s.trim()) },
    backNums() { return this.set.backNumbers ? this.set.backNumbers.split(',').map(s => s.trim()) : (this.set.blueNumbers || '').split(',').map(s => s.trim()) },
    inputFront() { return (this.highlightData && this.highlightData.frontNumbers) || [] },
    inputBack() { return (this.highlightData && this.highlightData.backNumbers) || [] },
    frontHighlights() { return buildHighlights(this.frontNums, this.inputFront) },
    backHighlights() { return buildHighlights(this.backNums, this.inputBack) }
  }
}
</script>
