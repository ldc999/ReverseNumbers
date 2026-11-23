<template>
  <div class="config-panel panel">
    <h3>玩法规则</h3>
    <div class="rules-text">
      <p><strong>{{ name }}</strong></p>
      <p>{{ lbl.front }}：{{ lbl.frontCount }}个号码 / {{ frontMax }}选{{ lbl.frontCount }}</p>
      <p>{{ lbl.back }}：{{ lbl.backCount }}个号码 / {{ backMax }}选{{ lbl.backCount }}</p>
      <p class="rules-note">选号后与开奖号码比对，匹配越多中奖越高</p>
    </div>
  </div>
</template>

<script>
import { gameLabels } from '../../utils/gameHelper'

export default {
  name: 'GameRules',
  inject: ['game'],
  computed: {
    lbl() { return gameLabels(this.game) },
    name() { return this.game === 'ssq' ? '双色球' : '大乐透' },
    frontMax() { return this.game === 'ssq' ? 33 : 35 },
    backMax() { return this.game === 'ssq' ? 16 : 12 }
  }
}
</script>
