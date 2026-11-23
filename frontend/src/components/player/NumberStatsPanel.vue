<template>
  <div class="stats-panel panel">
    <h3 class="panel-title" @click="collapsed = !collapsed">
      <span class="toggle-arrow">{{ collapsed ? '▶' : '▼' }}</span>
      号码统计 (共 {{ totalSets }} 注)
    </h3>
    <div v-show="!collapsed">
      <div v-if="totalSets === 0" class="hint">暂无号码</div>
      <div v-else class="stats-zones">
        <div class="stats-zone">
          <div class="stats-zone-title">{{ lbl.front }}</div>
          <div class="stats-bars">
            <div v-for="(item, idx) in frontStats" :key="item.num" class="stats-row">
              <span class="stats-rank">{{ idx + 1 }}</span>
              <span class="stats-num" :class="{ hit: isFrontHit(item.num) }">{{ item.num }}</span>
              <div class="stats-bar-wrap">
                <div class="stats-bar" :class="{ hit: isFrontHit(item.num) }" :style="{ width: item.pct + '%' }"></div>
              </div>
              <span class="stats-count">{{ item.count }}次</span>
              <span class="stats-pct">{{ item.pct }}%</span>
            </div>
          </div>
          <div class="stats-csv">{{ frontCsv }}</div>
        </div>
        <div class="stats-zone">
          <div class="stats-zone-title">{{ lbl.back }}</div>
          <div class="stats-bars">
            <div v-for="(item, idx) in backStats" :key="item.num" class="stats-row">
              <span class="stats-rank">{{ idx + 1 }}</span>
              <span class="stats-num" :class="{ hit: isBackHit(item.num) }">{{ item.num }}</span>
              <div class="stats-bar-wrap">
                <div class="stats-bar" :class="{ hit: isBackHit(item.num) }" :style="{ width: item.pct + '%' }"></div>
              </div>
              <span class="stats-count">{{ item.count }}次</span>
              <span class="stats-pct">{{ item.pct }}%</span>
            </div>
          </div>
          <div class="stats-csv">{{ backCsv }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { gameLabels } from '../../utils/gameHelper'

export default {
  name: 'NumberStatsPanel',
  inject: ['game'],
  props: {
    playerSets: { type: Array, default: () => [] },
    drawFront: { type: Array, default: () => [] },
    drawBack: { type: Array, default: () => [] },
    highlightData: { type: Object, default: null }
  },
  data() { return { collapsed: false } },
  computed: {
    lbl() { return gameLabels(this.game) },
    totalSets() { return this.playerSets.length },
    total() { return this.playerSets.length || 1 },
    frontDrawSet() { return new Set((this.drawFront && this.drawFront.length > 0) ? this.drawFront : (this.highlightData && this.highlightData.frontNumbers) || []) },
    backDrawSet() { return new Set((this.drawBack && this.drawBack.length > 0) ? this.drawBack : (this.highlightData && this.highlightData.backNumbers) || []) },
    backDrawSet() { return new Set(this.drawBack || []) },
    frontStats() {
      const max = this.game === 'ssq' ? 33 : 35
      const map = {}
      for (let i = 1; i <= max; i++) map[i] = 0
      for (const set of this.playerSets) {
        const nums = (set.frontNumbers || set.redNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))
        for (const n of nums) { if (map[n] !== undefined) map[n]++ }
      }
      return Object.entries(map).map(([num, cnt]) => ({
        num: String(num).padStart(2, '0'), count: cnt,
        pct: (cnt / this.total * 100).toFixed(1)
      })).sort((a, b) => b.count - a.count)
    },
    backStats() {
      const max = this.game === 'ssq' ? 16 : 12
      const map = {}
      for (let i = 1; i <= max; i++) map[i] = 0
      for (const set of this.playerSets) {
        const nums = (set.backNumbers || set.blueNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))
        for (const n of nums) { if (map[n] !== undefined) map[n]++ }
      }
      return Object.entries(map).map(([num, cnt]) => ({
        num: String(num).padStart(2, '0'), count: cnt,
        pct: (cnt / this.total * 100).toFixed(1)
      })).sort((a, b) => b.count - a.count)
    },
    frontCsv() { return this.frontStats.map(s => s.num).join(',') },
    backCsv() { return this.backStats.map(s => s.num).join(',') }
  },
  methods: {
    isFrontHit(num) { return this.frontDrawSet.has(parseInt(num)) },
    isBackHit(num) { return this.backDrawSet.has(parseInt(num)) }
  }
}
</script>
