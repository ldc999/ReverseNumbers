<template>
  <div class="number-square-group">
    <div class="zone-section">
      <span class="zone-label">{{ frontLabel }}</span>
      <div class="squares-row">
        <NumberSquare
          v-for="(num, idx) in frontNumbers"
          :key="'front-' + idx"
          :value="num"
          :highlighted="isFrontHighlighted(idx)"
          zone="front"
          :editable="editable"
          :disabled="disabled"
          :small="small"
          @input="onFrontChange(idx, $event)"
        />
      </div>
    </div>
    <div class="zone-divider"></div>
    <div class="zone-section">
      <span class="zone-label">{{ backLabel }}</span>
      <div class="squares-row">
        <NumberSquare
          v-for="(num, idx) in backNumbers"
          :key="'back-' + idx"
          :value="num"
          :highlighted="isBackHighlighted(idx)"
          zone="back"
          :editable="editable"
          :disabled="disabled"
          :small="small"
          @input="onBackChange(idx, $event)"
        />
      </div>
    </div>
  </div>
</template>

<script>
import NumberSquare from './NumberSquare.vue'

export default {
  name: 'NumberSquareGroup',
  components: { NumberSquare },
  props: {
    frontNumbers: {
      type: Array,
      default: () => ['', '', '', '', '']
    },
    backNumbers: {
      type: Array,
      default: () => ['', '']
    },
    frontLabel: { type: String, default: '前区' },
    backLabel: { type: String, default: '后区' },
    frontHighlights: {
      type: Object,
      default: () => ({})
    },
    backHighlights: {
      type: Object,
      default: () => ({})
    },
    editable: {
      type: Boolean,
      default: false
    },
    disabled: {
      type: Boolean,
      default: false
    },
    small: {
      type: Boolean,
      default: false
    },
    // If provided, only highlight when this player set index is in the match list
    playerSetIndex: {
      type: Number,
      default: null
    }
  },
  methods: {
    isFrontHighlighted(index) {
      const num = this.frontNumbers[index]
      if (!num) return false
      const key = String(num).padStart(2, '0')
      const match = this.frontHighlights[key]
      if (!match || match.length === 0) return false
      // If playerSetIndex is specified, check if it's in the match list
      if (this.playerSetIndex !== null) {
        return match.includes(this.playerSetIndex)
      }
      return true
    },
    isBackHighlighted(index) {
      const num = this.backNumbers[index]
      if (!num) return false
      const key = String(num).padStart(2, '0')
      const match = this.backHighlights[key]
      if (!match || match.length === 0) return false
      if (this.playerSetIndex !== null) {
        return match.includes(this.playerSetIndex)
      }
      return true
    },
    onFrontChange(index, value) {
      const copy = [...this.frontNumbers]
      copy.splice(index, 1, value)
      this.$emit('update:frontNumbers', copy)
    },
    onBackChange(index, value) {
      const copy = [...this.backNumbers]
      copy.splice(index, 1, value)
      this.$emit('update:backNumbers', copy)
    }
  }
}
</script>
