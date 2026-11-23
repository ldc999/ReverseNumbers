<template>
  <div
    class="number-square"
    :class="{
      'highlighted': highlighted,
      'front': zone === 'front',
      'back': zone === 'back',
      'disabled': disabled,
      'small': small
    }"
  >
    <input
      v-if="editable && !disabled"
      class="square-input"
      :value="value"
      :maxlength="2"
      @input="onInput"
      @keydown="onKeydown"
      placeholder="00"
    />
    <span v-else class="square-value">
      {{ displayValue }}
    </span>
  </div>
</template>

<script>
export default {
  name: 'NumberSquare',
  props: {
    value: {
      type: String,
      default: ''
    },
    highlighted: {
      type: Boolean,
      default: false
    },
    zone: {
      type: String,
      default: 'front',
      validator: v => ['front', 'back'].includes(v)
    },
    disabled: {
      type: Boolean,
      default: false
    },
    small: {
      type: Boolean,
      default: false
    },
    editable: {
      type: Boolean,
      default: false
    }
  },
  computed: {
    displayValue() {
      return this.value ? String(this.value).padStart(2, '0') : '--'
    }
  },
  methods: {
    onInput(e) {
      let val = e.target.value.replace(/\D/g, '')
      if (val.length > 2) val = val.slice(0, 2)
      this.$emit('input', val)
    },
    onKeydown(e) {
      // Allow navigation keys
      const navKeys = ['Tab', 'Enter', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End', 'Escape']
      if (navKeys.includes(e.key)) {
        return
      }
      if (e.key === 'Backspace' || e.key === 'Delete') {
        this.$emit('input', '')
        return
      }
      // Only allow digits
      if (!/^\d$/.test(e.key)) {
        e.preventDefault()
      }
    }
  }
}
</script>
