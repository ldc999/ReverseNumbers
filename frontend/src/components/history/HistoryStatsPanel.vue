<template>
  <div class="stats-panel panel" style="margin-top:8px">
    <h3 class="panel-title" @click="collapsed = !collapsed">
      <span class="toggle-arrow">{{ collapsed ? '▶' : '▼' }}</span>
      历史号码统计 (共 {{ filteredList.length }} 条)
    </h3>
    <div v-show="!collapsed">
      <div class="stats-date-row">
        <input type="datetime-local" v-model="startDate" class="input" style="width:190px" />
        <span>至</span>
        <input type="datetime-local" v-model="endDate" class="input" style="width:190px" />
        <button class="btn btn-sm" @click="startDate='';endDate=''" :disabled="!startDate && !endDate">清除</button>
      </div>
      <div v-if="filteredList.length === 0" class="hint">暂无数据</div>
      <div v-else class="stats-zones">
        <div class="stats-zone">
          <div class="stats-zone-title">{{ lbl.front }}</div>
          <div class="stats-bars">
            <div v-for="(item, idx) in frontStats" :key="item.num" class="stats-row">
              <span class="stats-rank">{{ idx + 1 }}</span>
              <span class="stats-num" :class="{ hit: isFrontHit(item.num) }">{{ item.num }}</span>
              <div class="stats-bar-wrap"><div class="stats-bar" :class="{ hit: isFrontHit(item.num) }" :style="{ width: item.pct + '%' }"></div></div>
              <span class="stats-count">{{ item.count }}次</span>
              <span class="stats-pct">{{ item.pct }}%</span>
            </div>
          </div>
        </div>
        <div class="stats-zone">
          <div class="stats-zone-title">{{ lbl.back }}</div>
          <div class="stats-bars">
            <div v-for="(item, idx) in backStats" :key="item.num" class="stats-row">
              <span class="stats-rank">{{ idx + 1 }}</span>
              <span class="stats-num" :class="{ hit: isBackHit(item.num) }">{{ item.num }}</span>
              <div class="stats-bar-wrap"><div class="stats-bar" :class="{ hit: isBackHit(item.num) }" :style="{ width: item.pct + '%' }"></div></div>
              <span class="stats-count">{{ item.count }}次</span>
              <span class="stats-pct">{{ item.pct }}%</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { gameLabels } from '../../utils/gameHelper'

export default {
  name: 'HistoryStatsPanel',
  inject: ['game'],
  props: {
    historyList: { type: Array, default: () => [] },
    highlightData: { type: Object, default: null }
  },
  data() {
    const now = new Date()
    const d30m = new Date(now.getTime() - 30 * 60000)
    const fmt = d => { const pad = n => String(n).padStart(2,'0'); return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}` }
    return { collapsed: true, startDate: fmt(d30m), endDate: '' }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    frontHitSet() { return new Set((this.highlightData && this.highlightData.frontNumbers) || []) },
    backHitSet() { return new Set((this.highlightData && this.highlightData.backNumbers) || []) },
    filteredList() {
      if (!this.startDate && !this.endDate) return this.historyList
      return this.historyList.filter(item => {
        const d = item.createdAt ? new Date(item.createdAt) : null
        if (!d) return false
        if (this.startDate && d < new Date(this.startDate)) return false
        if (this.endDate && d > new Date(this.endDate)) return false
        return true
      })
    },
    total() { return this.filteredList.length || 1 },
    frontMax() { return this.game === 'ssq' ? 33 : 35 },
    backMax() { return this.game === 'ssq' ? 16 : 12 },
    frontStats() {
      const map = {}
      for (let i = 1; i <= this.frontMax; i++) map[i] = 0
      for (const item of this.filteredList) {
        for (const n of item.frontNumbers) { if (map[n] !== undefined) map[n]++ }
      }
      return Object.entries(map).map(([n, c]) => ({
        num: String(n).padStart(2, '0'), count: c,
        pct: (c / this.total * 100).toFixed(1)
      })).sort((a, b) => b.count - a.count)
    },
    backStats() {
      const map = {}
      for (let i = 1; i <= this.backMax; i++) map[i] = 0
      for (const item of this.filteredList) {
        for (const n of item.backNumbers) { if (map[n] !== undefined) map[n]++ }
      }
      return Object.entries(map).map(([n, c]) => ({
        num: String(n).padStart(2, '0'), count: c,
        pct: (c / this.total * 100).toFixed(1)
      })).sort((a, b) => b.count - a.count)
    }
  },
  methods: {
    isFrontHit(num) { return this.frontHitSet.has(parseInt(num)) },
    isBackHit(num) { return this.backHitSet.has(parseInt(num)) }
  }
}
</script>
