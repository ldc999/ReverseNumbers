<template>
  <div class="main-view">
    <AppHeader />
    <div class="main-content">
      <div class="sidebar">
                <IssueSelector />
        <button class="btn btn-primary trend-entry-btn" @click="showTrend = true">双色球历史开奖走势</button>

        <div class="config-panel panel">
          <h3>参数设置 (或关系)</h3>
          <div v-for="(c, i) in constraints" :key="i" class="config-item constraint-row">
            <span>{{ lbl.front }}≤</span><input type="number" v-model.number="c.f" min="0" :max="lbl.frontCount"
              class="input-narrow" />
            <span>{{ lbl.back }}≤</span><input type="number" v-model.number="c.b" min="0" :max="lbl.backCount"
              class="input-narrow" />
            <button class="btn btn-sm btn-danger" @click="removeConstraint(i)"
              :disabled="constraints.length <= 1">×</button>
          </div>
          <button class="btn btn-sm" @click="addConstraint">+ 添加条件</button>
          <div class="config-item" style="margin-top:8px">
            <label><input type="checkbox" v-model="blindMode" /> 闭眼模式（隐藏开奖）</label>
          </div>
        </div>
        <HistoryConstraintConfig />

        <NumberStatsPanel :playerSets="playerSets" :drawFront="drawFront" :drawBack="drawBack"
          :highlightData="highlightData" />

        <GameRules />

      </div>
      <LotteryTrendModal v-if="showTrend" @close="showTrend = false" />
      <div class="content-area">
        <NumberInputPanel />
        <PlayerNumberInput :highlightData="highlightData" />
      </div>
      <div class="sidebar-right">
        <FavoriteNumbersPanel />
        <RetainedNumbersPanel :highlightData="highlightData" />
        <HistoryPanel :highlightData="highlightData" />
      </div>
    </div>
  </div>
</template>

<script>
import AppHeader from '../components/layout/AppHeader.vue'
import GameRules from '../components/layout/GameRules.vue'
import IssueSelector from '../components/issue/IssueSelector.vue'
import PlayerNumberInput from '../components/player/PlayerNumberInput.vue'
import NumberInputPanel from '../components/generator/NumberInputPanel.vue'
import HistoryPanel from '../components/history/HistoryPanel.vue'
import RetainedNumbersPanel from '../components/history/RetainedNumbersPanel.vue'
import FavoriteNumbersPanel from '../components/history/FavoriteNumbersPanel.vue'
import NumberStatsPanel from '../components/player/NumberStatsPanel.vue'
import HistoryConstraintConfig from '../components/generator/HistoryConstraintConfig.vue'
import LotteryTrendModal from '../components/lottery/LotteryTrendModal.vue'
import { ns, gameLabels } from '../utils/gameHelper'

export default {
  name: 'SsqView',
  components: { AppHeader, GameRules, IssueSelector, PlayerNumberInput, NumberInputPanel, HistoryPanel, RetainedNumbersPanel, FavoriteNumbersPanel, NumberStatsPanel, HistoryConstraintConfig, LotteryTrendModal },
  provide() { return { game: 'ssq' } },
  data() { return { showTrend: false } },
  computed: {
    playerSets() { return this.$store.state.ssq.ssqPlayerNumbers.playerSets },
    drawFront() { if (this.blindMode) return []; const s = (this.$store.state.ssq.ssqIssue.currentIssue || {}).winningRedNumbers || ''; return s.split(',').map(x => parseInt(x.trim())).filter(n => !isNaN(n)) },
    drawBack() { if (this.blindMode) return []; const s = (this.$store.state.ssq.ssqIssue.currentIssue || {}).winningBlueNumbers || ''; return s.split(',').map(x => parseInt(x.trim())).filter(n => !isNaN(n)) },
    lbl() { return gameLabels('ssq') },
    highlightData() { return this.$store.state.ssq.ssqGenerator.highlightResult },
    constraints() { return this.$store.state.ssq.ssqGenerator.highlightConstraints || [{ f: 4, b: 1 }] },
    blindMode: {
      get() { return this.$store.state.ssq.ssqGenerator.blindMode },
      set(v) { this.$store.commit('ssq/ssqGenerator/SET_BLIND_MODE', v) }
    }
  },
  methods: {
    addConstraint() { const c = [...this.constraints, { f: 3, b: 1 }]; this.$store.commit('ssq/ssqGenerator/SET_HIGHLIGHT_CONSTRAINTS', c) },
    removeConstraint(i) { const c = [...this.constraints]; c.splice(i, 1); this.$store.commit('ssq/ssqGenerator/SET_HIGHLIGHT_CONSTRAINTS', c) },
    async loadConfig() { await this.$store.dispatch('ssq/ssqGenerator/loadConfig') }
  },
  mounted() { this.loadConfig() }
}
</script>
