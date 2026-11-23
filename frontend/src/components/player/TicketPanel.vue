<template>
  <div class="fushi-input">
    <h4>票号输入</h4>
    <!-- Mode toggle -->
    <div class="adv-row">
      <label><input type="radio" v-model="mode" value="fushi" /> 复式</label>
      <label style="margin-left:12px"><input type="radio" v-model="mode" value="multi" /> 多注</label>
      <span class="ticket-pool-size" style="margin-left:auto">{{ lbl.front }}{{ liveFrontCount }}个 + {{ lbl.back }}{{ liveBackCount }}个</span>
    </div>

    <!-- 复式模式：选号 -->
    <div v-if="mode==='fushi'" class="fushi-zones">
      <div class="fushi-zone">
        <div class="fushi-zone-title">{{ lbl.front }}号码池 (选{{ lbl.frontCount }}个以上)</div>
        <div class="fushi-grid">
          <button v-for="n in frontMax" :key="'tf'+n" class="fushi-btn" :class="{ selected: frontPool.includes(n) }" @click="toggleFront(n)">{{ pad(n) }}</button>
        </div>
      </div>
      <div class="fushi-zone">
        <div class="fushi-zone-title">{{ lbl.back }}号码池 (选{{ lbl.backCount }}个以上)</div>
        <div class="fushi-grid">
          <button v-for="n in backMax" :key="'tb'+n" class="fushi-btn" :class="{ selected: backPool.includes(n) }" @click="toggleBack(n)">{{ pad(n) }}</button>
        </div>
      </div>
    </div>

    <!-- 多注模式：逐条输入 -->
    <div v-else class="multi-bets">
      <div v-for="(bet, i) in bets" :key="i" class="multi-bet-row">
        <span class="set-index">{{ i+1 }}</span>
        <NumberSquareGroup
          :editable="true"
          :frontNumbers.sync="bet.front" :backNumbers.sync="bet.back"
          :frontLabel="lbl.front" :backLabel="lbl.back"
        />
        <button class="btn btn-sm btn-danger" @click="bets.splice(i,1)">×</button>
      </div>
      <button class="btn btn-sm" @click="bets.push(emptyBet())">+ 添加注</button>
    </div>

    <!-- Constraints -->
    <div class="ticket-constraints">
      <div class="adv-row">
        <label class="adv-label">票号限制条件 (或关系):</label>
        <button class="btn btn-sm" @click="addConstraint">+ 添加</button>
      </div>
      <div v-for="(c, i) in constraints" :key="i" class="constraint-row">
        <span>{{ lbl.front }}</span>
        <input v-model.number="c.fMin" type="number" min="0" :max="lbl.frontCount" class="input-narrow" />
        <span>≤</span><span>{{ lbl.front }}</span><span>≤</span>
        <input v-model.number="c.f" type="number" min="0" :max="lbl.frontCount" class="input-narrow" />
        <span>{{ lbl.back }}</span>
        <input v-model.number="c.bMin" type="number" min="0" :max="lbl.backCount" class="input-narrow" />
        <span>≤</span><span>{{ lbl.back }}</span><span>≤</span>
        <input v-model.number="c.b" type="number" min="0" :max="lbl.backCount" class="input-narrow" />
        <button class="btn btn-sm btn-danger" @click="constraints.splice(i,1)">×</button>
      </div>
    </div>
    <div class="fushi-actions">
      <button class="btn btn-primary" :disabled="!canSave" @click="save">{{ saving ? '保存中...' : '保存票号' }}</button>
      <button class="btn" @click="$emit('close')">取消</button>
    </div>
  </div>
</template>

<script>
import NumberSquareGroup from '../common/NumberSquareGroup.vue'
import { ns, gameLabels, emptyNumbers } from '../../utils/gameHelper'

export default {
  name: 'TicketPanel',
  components: { NumberSquareGroup },
  inject: ['game'],
  data() {
    return {
      mode: 'fushi',
      frontPool: [], backPool: [],
      bets: [this.emptyBet()],
      constraints: [{ fMin: 0, f: 3, bMin: 0, b: 0 }],
      saving: false
    }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    issueId() { return this.$store.getters[ns(this.game, 'issue/currentIssueId')] },
    frontMax() { return this.game === 'ssq' ? 33 : 35 },
    backMax() { return this.game === 'ssq' ? 16 : 12 },
    liveFrontCount() {
      if (this.mode === 'fushi') return this.frontPool.length
      const s = new Set()
      for (const b of this.bets) for (const n of b.front) { const v = parseInt(n); if (!isNaN(v)) s.add(v) }
      return s.size
    },
    liveBackCount() {
      if (this.mode === 'fushi') return this.backPool.length
      const s = new Set()
      for (const b of this.bets) for (const n of b.back) { const v = parseInt(n); if (!isNaN(v)) s.add(v) }
      return s.size
    },
    canSave() {
      if (this.mode === 'fushi') {
        return this.frontPool.length >= this.lbl.frontCount && this.backPool.length >= this.lbl.backCount
      }
      return this.bets.length > 0 && this.bets.every(b =>
        b.front.filter(x => x !== '').length === this.lbl.frontCount &&
        b.back.filter(x => x !== '').length === this.lbl.backCount
      )
    }
  },
  methods: {
    pad(n) { return String(n).padStart(2, '0') },
    emptyBet() { const e = emptyNumbers(this.game); return { front: e.front, back: e.back } },
    toggle(arr, n) { const i = arr.indexOf(n); if (i >= 0) arr.splice(i, 1); else arr.push(n); arr.sort((a, b) => a - b) },
    toggleFront(n) { this.toggle(this.frontPool, n) },
    toggleBack(n) { this.toggle(this.backPool, n) },
    addConstraint() { this.constraints.push({ fMin: 0, f: 3, bMin: 0, b: 0 }) },
    async save() {
      if (!this.canSave) return
      this.saving = true
      try {
        const cons = this.constraints.filter(c => c.f >= 0 || c.b >= 0)
        if (this.mode === 'fushi') {
          await this.$store.dispatch(ns(this.game, 'tickets/saveTicket'), {
            issueId: this.issueId, frontPool: this.frontPool, backPool: this.backPool, constraints: cons, ticketType: 'fushi', addToPlayer: false
          })
        } else {
          // Multi mode: compute pool from all bets
          const fSet = new Set(), bSet = new Set()
          const bets = []
          for (const bet of this.bets) {
            const fStr = bet.front.filter(x => x !== '').map(n => String(parseInt(n)).padStart(2, '0')).join(',')
            const bStr = bet.back.filter(x => x !== '').map(n => String(parseInt(n)).padStart(2, '0')).join(',')
            bets.push({ frontNumbers: fStr, backNumbers: bStr })
            for (const n of bet.front) { const v = parseInt(n); if (!isNaN(v)) fSet.add(v) }
            for (const n of bet.back) { const v = parseInt(n); if (!isNaN(v)) bSet.add(v) }
          }
          await this.$store.dispatch(ns(this.game, 'tickets/saveTicket'), {
            issueId: this.issueId,
            frontPool: [...fSet].sort((a,b)=>a-b),
            backPool: [...bSet].sort((a,b)=>a-b),
            constraints: cons,
            bets, ticketType: 'multi', addToPlayer: true
          })
        }
        this.$emit('saved')
      } finally { this.saving = false }
    }
  }
}
</script>
