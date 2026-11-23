<template>
  <div class="number-input-panel panel">
    <h3>号码输入 / 随机生成</h3>
    <div v-if="!issueId" class="hint">请先选择期号</div>
    <div v-else class="panel-body">
      <NumberSquareGroup :editable="true" :frontNumbers.sync="currentFront" :backNumbers.sync="currentBack" :frontLabel="lbl.front" :backLabel="lbl.back" :frontHighlights="drawFrontHighlights" :backHighlights="drawBackHighlights" />
      <div class="panel-actions">
        <button class="btn btn-primary" @click="doRandomGenerate" :disabled="generating">{{ generating ? '生成中...' : '随机生成'+batchCount+'个' }}</button>
        <input v-model.number="batchCount" type="number" min="1" max="20" class="input-narrow" style="width:60px" title="同时生成数量(1-20)" />
        <label class="adv-toggle" style="margin-left:8px"><input type="checkbox" v-model="useTicket" /> 票限</label>
        <button class="btn btn-primary" @click="doHighlight" :disabled="highlighting">{{ highlighting ? '检测中...' : '高亮检测' }}</button>
        <!-- <button class="btn btn-success" @click="doSaveHistory" :disabled="!hasNumbers">保存到历史</button> -->
        <button class="btn" @click="doRetain" :disabled="!hasNumbers">保留号码</button>
        <button class="btn" @click="doAddToPlayer" :disabled="!hasNumbers">添加到玩家号码</button>
        <button class="btn" @click="showFavInput = true" :disabled="!hasNumbers">收藏号码</button>
        <button class="btn btn-sm" @click="showAdvanced = !showAdvanced">{{ showAdvanced ? '收起筛选' : '高级筛选' }}</button>
      </div>

      <!-- Advanced filter -->
      <div v-if="showAdvanced" class="advanced-options">
        <div class="adv-row">
          <label class="adv-toggle"><input type="checkbox" v-model="advEnabled" /> 启用筛选条件</label>
        </div>
        <div :class="{ disabled: !advEnabled }">
          <!-- Exclude front -->
          <div class="adv-zone">
            <div class="adv-zone-head" @click="advOpen = advOpen === 'ef' ? '' : 'ef'">
              <span>{{ advOpen === 'ef' ? '▼' : '▶' }} 排除{{ lbl.front }}</span>
              <button class="btn btn-sm" style="margin-left:4px" @click.stop="randomExcludeOne" title="随机选一注，从中随机取一个前区号，已取过的不再取">随1法</button>
              <button class="btn btn-sm" style="margin-left:2px" @click.stop="randomExcludeWeight" title="按前区数字出现的百分比权重随机取一个">权重法</button>
              <button class="btn btn-sm" style="margin-left:2px" @click.stop="randomExcludeFullOne" title="每注各取一个前区号，不去重，随机取一个">随全取1</button>
              <input v-model.number="eachCount" type="number" min="1" style="width:40px;margin-left:2px;padding:2px 4px;font-size:11px" placeholder="N" />
              <button class="btn btn-sm" style="margin-left:2px" @click.stop="randomExcludeEach" title="随机N注，每注取一个前区号，去重后赋值">随全法</button>
              <button class="btn btn-sm" style="margin-left:2px" @click.stop="randomExcludeEachOne" title="随全法生成去重组 → 从中随机取一个">随全取1</button>
              <button class="btn btn-sm" style="margin-left:4px" @click.stop="clearExFront">清空</button>
            </div>
            <div class="adv-excluded" v-if="excludedFront.length">{{ excludedFront.join(',') }}</div>
            <div v-show="advOpen === 'ef'">
              <div class="fushi-grid adv-grid">
                <button v-for="n in frontMax" :key="'ef'+n" class="fushi-btn" :class="{ selected: excludedFront.includes(n), flash: flashNum === n }" @click="toggleExFront(n)">{{ pad(n) }}</button>
              </div>
              <div class="remain-info" v-if="excludedFront.length">剩余可选: {{ remainingFront.join(',') }}</div>
            </div>
          </div>
          <!-- Exclude back -->
          <div class="adv-zone">
            <div class="adv-zone-head" @click="advOpen = advOpen === 'eb' ? '' : 'eb'">
              <span>{{ advOpen === 'eb' ? '▼' : '▶' }} 排除{{ lbl.back }} <em>{{ excludedBack.length ? excludedBack.join(',') : '无' }}</em></span>
            </div>
            <div v-show="advOpen === 'eb'" class="fushi-grid adv-grid">
              <button v-for="n in backMax" :key="'eb'+n" class="fushi-btn" :class="{ selected: excludedBack.includes(n) }" @click="toggleExBack(n)">{{ pad(n) }}</button>
            </div>
          </div>
          <!-- Require front -->
          <div class="adv-zone">
            <div class="adv-zone-head" @click="advOpen = advOpen === 'rf' ? '' : 'rf'">
              <span>{{ advOpen === 'rf' ? '▼' : '▶' }} 必选{{ lbl.front }} <em>{{ requiredFront.length ? requiredFront.join(',') : '无' }}</em></span>
            </div>
            <div v-show="advOpen === 'rf'" class="fushi-grid adv-grid">
              <button v-for="n in frontMax" :key="'rf'+n" class="fushi-btn" :class="{ selected: requiredFront.includes(n) }" @click="toggleReqFront(n)">{{ pad(n) }}</button>
            </div>
          </div>
          <!-- Require back -->
          <div class="adv-zone">
            <div class="adv-zone-head" @click="advOpen = advOpen === 'rb' ? '' : 'rb'">
              <span>{{ advOpen === 'rb' ? '▼' : '▶' }} 必选{{ lbl.back }} <em>{{ requiredBack.length ? requiredBack.join(',') : '无' }}</em></span>
            </div>
            <div v-show="advOpen === 'rb'" class="fushi-grid adv-grid">
              <button v-for="n in backMax" :key="'rb'+n" class="fushi-btn" :class="{ selected: requiredBack.includes(n) }" @click="toggleReqBack(n)">{{ pad(n) }}</button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="showFavInput" class="fav-input-row">
        <input v-model="favLabel" class="input" placeholder="收藏备注（可选）" @keyup.enter="doFavorite" />
        <button class="btn btn-primary btn-sm" @click="doFavorite">确认收藏</button>
        <button class="btn btn-sm" @click="showFavInput = false">取消</button>
      </div>
      <div v-if="lastGenIter > 0 || (lastResult && lastResult.iterationCount === 0 && lastResult.frontMatchDetails)" class="result-info">
        <span v-if="lastGenIter > 0">随机生成成功，迭代 {{ lastGenIter }} 次</span>
        <span v-else-if="lastResult">高亮检测完成</span>
      </div>

      <!-- 随全取1弹窗 -->
      <div v-if="showFlashModal && flashModalData" class="modal-overlay" @click.self="showFlashModal=false">
        <div class="modal" style="max-width:500px">
          <h4>随全取1 详情</h4>
          <div class="flash-modal-body">
            <div v-for="(item, i) in flashModalData.details" :key="i" class="flash-modal-row">
              <span class="flash-set">{{ item.set }}</span>
              <span class="flash-arrow">→</span>
              <span class="flash-pick">{{ item.pick }}</span>
            </div>
          </div>
          <div class="flash-modal-result">
            随机取到: <strong>{{ flashModalData.pool }}</strong><br/>
            最终取到: <strong style="color:#e65100;font-size:18px">{{ flashModalData.final }}</strong>
          </div>
          <button class="btn btn-primary" style="margin-top:12px" @click="showFlashModal=false">关闭</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import NumberSquareGroup from '../common/NumberSquareGroup.vue'
import { ns, gameLabels } from '../../utils/gameHelper'

export default {
  name: 'NumberInputPanel',
  components: { NumberSquareGroup },
  inject: ['game'],
  data() {
    return {
      generating: false, highlighting: false,
      lastGenIter: 0, batchCount: 1, doneCount: 0, useTicket: false,
      showFavInput: false, favLabel: '',
      showAdvanced: false, advOpen: '',
      advEnabled: true,
      excludedFront: [], excludedBack: [],
      requiredFront: [], requiredBack: [],
      usedSetsByOne: [], eachCount: 5, flashNum: null,
      showFlashModal: false, flashModalData: null
    }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    issueId() { return this.$store.getters[ns(this.game, 'issue/currentIssueId')] },
    advKey() { return `adv_${this.game}_${this.issueId || 0}` },
    frontMax() { return this.game === 'ssq' ? 33 : 35 },
    backMax() { return this.game === 'ssq' ? 16 : 12 },
    remainingFront() {
      const all = []
      for (let i = 1; i <= this.frontMax; i++) {
        if (!this.excludedFront.includes(i)) all.push(this.pad(i))
      }
      return all
    },
    currentFront: {
      get() { return this.game === 'ssq' ? this.$store.state.ssq.ssqGenerator.currentFrontNumbers : this.$store.state.generator.currentFrontNumbers },
      set(v) { this.$store.commit(ns(this.game, 'generator/SET_CURRENT_FRONT'), v) }
    },
    currentBack: {
      get() { return this.game === 'ssq' ? this.$store.state.ssq.ssqGenerator.currentBackNumbers : this.$store.state.generator.currentBackNumbers },
      set(v) { this.$store.commit(ns(this.game, 'generator/SET_CURRENT_BACK'), v) }
    },
    currentIssue() {
      return this.game === 'ssq' ? this.$store.state.ssq.ssqIssue.currentIssue : this.$store.state.issue.currentIssue
    },
    blindMode() { return this.game === 'ssq' ? this.$store.state.ssq.ssqGenerator.blindMode : this.$store.state.generator.blindMode },
    drawFront() {
      if (this.blindMode || !this.currentIssue) return []
      const s = this.currentIssue.winningFrontNumbers || this.currentIssue.winningRedNumbers || ''
      return s.split(',').map(x => parseInt(x.trim())).filter(n => !isNaN(n))
    },
    drawBack() {
      if (this.blindMode || !this.currentIssue) return []
      const s = this.currentIssue.winningBackNumbers || this.currentIssue.winningBlueNumbers || ''
      return s.split(',').map(x => parseInt(x.trim())).filter(n => !isNaN(n))
    },
    drawFrontHighlights() {
      if (!this.drawFront.length) return {}
      const set = new Set(this.drawFront)
      const result = {}
      for (const n of this.currentFront) {
        const num = parseInt(n)
        if (!isNaN(num) && set.has(num)) result[String(num).padStart(2, '0')] = [0]
      }
      return result
    },
    drawBackHighlights() {
      if (!this.drawBack.length) return {}
      const set = new Set(this.drawBack)
      const result = {}
      for (const n of this.currentBack) {
        const num = parseInt(n)
        if (!isNaN(num) && set.has(num)) result[String(num).padStart(2, '0')] = [0]
      }
      return result
    },
    lastResult() {
      return this.game === 'ssq' ? this.$store.state.ssq.ssqGenerator.highlightResult : this.$store.state.generator.highlightResult
    },
    hasNumbers() {
      const f = this.currentFront.filter(n => n !== '')
      const b = this.currentBack.filter(n => n !== '')
      const cfg = gameLabels(this.game)
      return f.length === cfg.frontCount && b.length === cfg.backCount
    }
  },
  watch: {
    issueId() { this.loadAdvanced() },
    advEnabled() { this.saveAdvanced() },
    excludedFront() { this.saveAdvanced() },
    excludedBack() { this.saveAdvanced() },
    requiredFront() { this.saveAdvanced() },
    requiredBack() { this.saveAdvanced() }
  },
  methods: {
    dispatch(nsPath, payload) { return this.$store.dispatch(ns(this.game, nsPath), payload) },
    commit(nsPath, payload) { this.$store.commit(ns(this.game, nsPath), payload) },
    pad(n) { return String(n).padStart(2, '0') },
    toggle(arr, n) { const i = arr.indexOf(n); if (i >= 0) arr.splice(i, 1); else arr.push(n); arr.sort((a, b) => a - b) },
    toggleExFront(n) { this.toggle(this.excludedFront, n) },
    addExcluded(n) { if (!this.excludedFront.includes(n)) this.excludedFront.push(n); this.excludedFront.sort((a, b) => a - b) },
    toggleExBack(n) { this.toggle(this.excludedBack, n) },
    toggleReqFront(n) { this.toggle(this.requiredFront, n) },
    toggleReqBack(n) { this.toggle(this.requiredBack, n) },
    clearExFront() { if (confirm('确定清空所有排除号码吗？')) { this.excludedFront = []; this.usedSetsByOne = [] } },
    getPlayerSets() {
      return this.game === 'ssq' ? this.$store.state.ssq.ssqPlayerNumbers.playerSets : this.$store.state.playerNumbers.playerSets
    },
    parseFrontNums(set) {
      return (set.frontNumbers || set.redNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))
    },
    // 随1法：随机选一注（取过的注不再取），从中随机取一个前区号
    randomExcludeOne() {
      const sets = this.getPlayerSets()
      if (sets.length === 0) { alert('暂无玩家号码'); return }
      const unused = sets.filter((_, i) => !this.usedSetsByOne.includes(i))
      if (unused.length === 0) { alert('所有注都已被随1法取过，请点清空重置'); return }
      const set = unused[Math.floor(Math.random() * unused.length)]
      const setIndex = sets.indexOf(set)
      const nums = this.parseFrontNums(set)
      const pick = nums[Math.floor(Math.random() * nums.length)]
      this.usedSetsByOne.push(setIndex)
      this.addExcluded(pick)
      this.flashNum = pick
      setTimeout(() => { this.flashNum = null }, 800)
    },
    // 权重法：按前区数字出现的次数作为权重，随机取一个
    randomExcludeWeight() {
      const sets = this.getPlayerSets()
      const weightMap = {}
      for (const set of sets) {
        for (const n of this.parseFrontNums(set)) {
          weightMap[n] = (weightMap[n] || 0) + 1
        }
      }
      const available = Object.entries(weightMap)
        .map(([n, w]) => ({ num: parseInt(n), weight: w }))
      if (available.length === 0) { alert('没有可排除的号码了'); return }
      const totalWeight = available.reduce((s, a) => s + a.weight, 0)
      let r = Math.random() * totalWeight
      for (const a of available) {
        r -= a.weight
        if (r <= 0) {
          this.addExcluded(a.num)
          this.flashNum = a.num
          setTimeout(() => { this.flashNum = null }, 800)
          return
        }
      }
    },
    // 随全取1：每注各取一个前区号（不去重），从结果池中随机取一个
    randomExcludeFullOne() {
      const sets = this.getPlayerSets()
      if (sets.length === 0) { alert('暂无玩家号码'); return }
      const pool = []
      for (const set of sets) {
        const nums = this.parseFrontNums(set)
        if (nums.length > 0) pool.push(nums[Math.floor(Math.random() * nums.length)])
      }
      if (pool.length === 0) { alert('没有可选号码'); return }
      const pick = pool[Math.floor(Math.random() * pool.length)]
      this.addExcluded(pick)
      this.flashNum = pick
      setTimeout(() => { this.flashNum = null }, 800)
    },
    // 随全法：随机取N注，每注取一个前区号，去重后赋值到排除列表
    randomExcludeEach() {
      const sets = this.getPlayerSets()
      if (sets.length === 0) { alert('暂无玩家号码'); return }
      const n = Math.min(this.eachCount || 5, sets.length)
      const shuffled = [...sets].sort(() => Math.random() - 0.5).slice(0, n)
      const picked = new Set()
      for (const set of shuffled) {
        const nums = this.parseFrontNums(set)
        if (nums.length > 0) {
          picked.add(nums[Math.floor(Math.random() * nums.length)])
        }
      }
      if (picked.size === 0) { alert('没有可选号码'); return }
      for (const n of picked) this.addExcluded(n)
    },
    // 随全取1：随全法生成去重组 → 从中随机取一个
    randomExcludeEachOne() {
      const sets = this.getPlayerSets()
      if (sets.length === 0) { alert('暂无玩家号码'); return }
      const n = Math.min(this.eachCount || 5, sets.length)
      const shuffled = [...sets].sort(() => Math.random() - 0.5).slice(0, n)
      const details = []
      const pool = []
      for (const set of shuffled) {
        const nums = this.parseFrontNums(set)
        if (nums.length > 0) {
          const r = nums[Math.floor(Math.random() * nums.length)]
          pool.push(r)
          details.push({ set: this.padJoin(nums), pick: this.padJoin([r]) })
        }
      }
      if (pool.length === 0) { alert('没有可选号码'); return }
      const pick = pool[Math.floor(Math.random() * pool.length)]
      this.flashModalData = {
        details,
        pool: pool.map(x => this.padJoin([x])).join(', '),
        final: this.padJoin([pick])
      }
      this.showFlashModal = true
      this.addExcluded(pick)
      this.flashNum = pick
      setTimeout(() => { this.flashNum = null }, 800)
    },
    padJoin(arr) { return arr.map(n => String(n).padStart(2, '0')).join(',') },

    loadAdvanced() {
      try {
        const saved = JSON.parse(localStorage.getItem(this.advKey) || '{}')
        this.advEnabled = saved.en !== false
        this.excludedFront = saved.ef || []
        this.excludedBack = saved.eb || []
        this.requiredFront = saved.rf || []
        this.requiredBack = saved.rb || []
      } catch { /* ignore */ }
    },
    saveAdvanced() {
      localStorage.setItem(this.advKey, JSON.stringify({
        en: this.advEnabled,
        ef: this.excludedFront, eb: this.excludedBack,
        rf: this.requiredFront, rb: this.requiredBack
      }))
    },

    async doRandomGenerate() {
      if (!this.issueId) return
      this.generating = true
      this.doneCount = 0
      const count = Math.min(Math.max(this.batchCount || 1, 1), 20)
      try {
        const tickets = this.game === 'ssq' ? this.$store.state.ssq.ssqTickets.ticketList : this.$store.state.tickets.ticketList
        const ticketConstraints = this.useTicket
          ? tickets.filter(t => t.constraints && t.constraints.length > 0).map(t => ({
              frontPool: t.frontPool, backPool: t.backPool, constraints: t.constraints
            }))
          : []
        const params = {
          issueId: this.issueId,
          excludedFront: this.advEnabled ? this.excludedFront : [],
          excludedBack: this.advEnabled ? this.excludedBack : [],
          requiredFront: this.advEnabled ? this.requiredFront : [],
          requiredBack: this.advEnabled ? this.requiredBack : [],
          ticketConstraints
        }
        const tasks = []
        for (let i = 0; i < count; i++) {
          tasks.push(this.dispatch('generator/generateRandom', { ...params }))
        }
        const results = await Promise.all(tasks)
        this.lastGenIter = 0
        for (const result of results) {
          if (result) {
            this.lastGenIter = Math.max(this.lastGenIter, result.iterationCount || 0)
            this.doneCount++
            if (this.doneCount === 1) {
              this.commit('generator/SET_CURRENT_FRONT', result.frontNumbers.map(n => String(n).padStart(2, '0')))
              this.commit('generator/SET_CURRENT_BACK', result.backNumbers.map(n => String(n).padStart(2, '0')))
            }
            await this.dispatch('history/saveToHistory', {
              issueId: this.issueId,
              frontNumbers: result.frontNumbers,
              backNumbers: result.backNumbers,
              isGenerated: true,
              iterationCount: result.iterationCount || 0,
              matchDetails: ''
            })
          }
        }
      } finally { this.generating = false }
    },
    async doHighlight() {
      if (!this.issueId) return
      const frontNums = this.currentFront.filter(n => n !== '').map(n => parseInt(n))
      const backNums = this.currentBack.filter(n => n !== '').map(n => parseInt(n))
      const cfg = gameLabels(this.game)
      if (frontNums.length !== cfg.frontCount || backNums.length !== cfg.backCount) {
        alert(`请填写完整的${cfg.front}${cfg.frontCount}个号码和${cfg.back}${cfg.backCount}个号码`)
        return
      }
      this.highlighting = true
      try { await this.dispatch('generator/highlightManual', { issueId: this.issueId, frontNumbers: frontNums, backNumbers: backNums }) }
      finally { this.highlighting = false }
    },
    async doSaveHistory() {
      if (!this.lastResult) return
      await this.dispatch('history/saveToHistory', {
        issueId: this.issueId,
        frontNumbers: this.lastResult.frontNumbers,
        backNumbers: this.lastResult.backNumbers,
        isGenerated: this.lastResult.iterationCount > 0,
        matchDetails: JSON.stringify({ front: this.lastResult.frontMatchDetails, back: this.lastResult.backMatchDetails })
      })
    },
    async doRetain() {
      const frontNums = this.currentFront.filter(n => n !== '').map(n => parseInt(n))
      const backNums = this.currentBack.filter(n => n !== '').map(n => parseInt(n))
      const cfg = gameLabels(this.game)
      if (frontNums.length !== cfg.frontCount || backNums.length !== cfg.backCount) { alert('请填写完整号码'); return }
      await this.dispatch('retainedNumbers/saveRetained', { issueId: this.issueId, frontNumbers: frontNums, backNumbers: backNums })
    },
    async doAddToPlayer() {
      const frontNums = this.currentFront.filter(n => n !== '').map(n => parseInt(n))
      const backNums = this.currentBack.filter(n => n !== '').map(n => parseInt(n))
      const cfg = gameLabels(this.game)
      if (frontNums.length !== cfg.frontCount || backNums.length !== cfg.backCount) { alert('请填写完整号码'); return }
      if (!confirm('确定将当前号码添加到玩家号码列表吗？')) return
      const f = frontNums.map(n => String(n).padStart(2, '0')).join(',')
      const b = backNums.map(n => String(n).padStart(2, '0')).join(',')
      await this.dispatch('playerNumbers/addPlayerSet', { issueId: this.issueId, frontNumbers: f, backNumbers: b })
    },
    async doFavorite() {
      const frontNums = this.currentFront.filter(n => n !== '').map(n => parseInt(n))
      const backNums = this.currentBack.filter(n => n !== '').map(n => parseInt(n))
      const cfg = gameLabels(this.game)
      if (frontNums.length !== cfg.frontCount || backNums.length !== cfg.backCount) { alert('请填写完整号码'); return }
      await this.dispatch('favorites/saveFavorite', { frontNumbers: frontNums, backNumbers: backNums, label: this.favLabel.trim() || null })
      this.favLabel = ''; this.showFavInput = false
    }
  }
}
</script>
