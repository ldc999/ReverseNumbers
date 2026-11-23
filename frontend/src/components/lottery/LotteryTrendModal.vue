<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="trend-modal">
      <div class="trend-head">
        <h3>{{ gameName }} 历史开奖走势</h3>
        <select v-model.number="count" class="input" style="width:110px" @change="load()">
          <option :value="30">最近30期</option>
          <option :value="50">最近50期</option>
          <option :value="100">最近100期</option>
          <option :value="200">最近200期</option>
          <option :value="300">最近300期</option>
        </select>
        <button class="btn btn-primary btn-sm" :disabled="loading" @click="load(true)">
          {{ loading ? '加载中...' : '刷新数据' }}
        </button>
        <span class="sel-summary">
          {{ lbl.front }} <b :class="{ over: selFront.length > lbl.frontCount }">{{ selFront.length }}</b>/{{ lbl.frontCount }}
          &nbsp;{{ lbl.back }} <b :class="{ over: selBack.length > lbl.backCount }">{{ selBack.length }}</b>/{{ lbl.backCount }}
        </span>
        <button class="btn btn-sm" @click="clearSel" :disabled="!selFront.length && !selBack.length">清空选号</button>
        <button class="btn btn-sm" @click="copySel" :disabled="!selFront.length && !selBack.length">复制</button>
        <button class="btn btn-sm btn-primary" @click="fillToInput" :disabled="!canFill">回填到输入方格</button>
        <span class="trend-source">数据来源：500.com　缓存 {{ cachedCount }} 期</span>
        <button class="btn btn-sm" @click="$emit('close')">关闭</button>
      </div>

      <div v-if="loading && !draws.length" class="hint">正在从 500.com 获取开奖数据...</div>
      <div v-else-if="!draws.length" class="hint">暂无数据</div>

      <div v-else class="trend-body">
        <table class="trend-table">
          <thead>
            <tr>
              <th class="sticky-col">期号</th>
              <th v-for="n in frontMax" :key="'hf'+n" class="num-col">{{ pad(n) }}</th>
              <th class="sep-col"></th>
              <th v-for="n in backMax" :key="'hb'+n" class="num-col back">{{ pad(n) }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="d in rows" :key="d.issueNumber">
              <td class="sticky-col issue">
                <span class="issue-no">{{ d.issueNumber }}</span>
                <span class="issue-date">{{ (d.drawDate || '').slice(5) }}</span>
              </td>
              <td v-for="n in frontMax" :key="'f'+d.issueNumber+n"
                  class="num-col" :class="{ hit: d.front.has(n) }">
                <span v-if="d.front.has(n)" class="ball front">{{ pad(n) }}</span>
              </td>
              <td class="sep-col"></td>
              <td v-for="n in backMax" :key="'b'+d.issueNumber+n"
                  class="num-col back" :class="{ hit: d.back.has(n) }">
                <span v-if="d.back.has(n)" class="ball back">{{ pad(n) }}</span>
              </td>
            </tr>
            <!-- 手动选号行：紧接最新一期，滚动时固定在底部 -->
            <tr class="sel-row">
              <td class="sticky-col">
                选号
                <span v-if="selFront.length || selBack.length" class="sel-clear" @click="clearSel">清除</span>
              </td>
              <td v-for="n in frontMax" :key="'sf'+n" class="num-col sel-cell"
                  :class="{ selected: selFront.includes(n) }" @click="toggleSel(selFront, n)">
                {{ pad(n) }}
              </td>
              <td class="sep-col"></td>
              <td v-for="n in backMax" :key="'sb'+n" class="num-col back sel-cell"
                  :class="{ selected: selBack.includes(n) }" @click="toggleSel(selBack, n)">
                {{ pad(n) }}
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="stat-row">
              <td class="sticky-col">出现次数</td>
              <td v-for="n in frontMax" :key="'cf'+n" class="num-col">{{ frontCount[n] }}</td>
              <td class="sep-col"></td>
              <td v-for="n in backMax" :key="'cb'+n" class="num-col back">{{ backCount[n] }}</td>
            </tr>
            <tr class="stat-row">
              <td class="sticky-col">当前遗漏</td>
              <td v-for="n in frontMax" :key="'of'+n" class="num-col"
                  :class="{ warn: frontOmission[n] >= 15 }">{{ frontOmission[n] }}</td>
              <td class="sep-col"></td>
              <td v-for="n in backMax" :key="'ob'+n" class="num-col back"
                  :class="{ warn: backOmission[n] >= 8 }">{{ backOmission[n] }}</td>
            </tr>
          </tfoot>
        </table>
      </div>

      <!-- 分区统计 -->
      <div v-if="draws.length" class="trend-zone-stats">
        <span v-for="z in zoneStats" :key="z.label" class="zone-chip">
          {{ z.label }} 共{{ z.total }}次 ({{ z.pct }}%)
        </span>
      </div>
    </div>
  </div>
</template>

<script>
import { gameLabels } from '../../utils/gameHelper'
import lotteryApi from '../../api/lottery'

export default {
  name: 'LotteryTrendModal',
  inject: ['game'],
  props: {
    defaultCount: { type: Number, default: 100 }
  },
  data() {
    return {
      loading: false, draws: [], frontSets: [], backSets: [],
      frontMax: 35, backMax: 12, cachedCount: 0, count: this.defaultCount,
      selFront: [], selBack: []
    }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    gameName() { return this.game === 'ssq' ? '双色球' : '大乐透' },
    canFill() {
      return this.selFront.length === this.lbl.frontCount && this.selBack.length === this.lbl.backCount
    },
    // 展示用：期号从早到晚（最新一期排在最下面）
    // 内部 draws/frontSets/backSets 保持最新在前，遗漏与次数统计依赖该顺序
    rows() {
      const out = []
      for (let i = this.draws.length - 1; i >= 0; i--) {
        out.push({ ...this.draws[i], front: this.frontSets[i], back: this.backSets[i] })
      }
      return out
    },
    frontCount() { return this.rangeCount(this.frontMax, this.frontSets) },
    backCount() { return this.rangeCount(this.backMax, this.backSets) },
    frontOmission() { return this.omissions(this.frontMax, this.frontSets) },
    backOmission() { return this.omissions(this.backMax, this.backSets) },
    zoneStats() {
      const zones = this.game === 'ssq'
        ? [{ label: `${this.lbl.front}低区 01-11`, lo: 1, hi: 11 }, { label: `${this.lbl.front}中区 12-22`, lo: 12, hi: 22 }, { label: `${this.lbl.front}高区 23-33`, lo: 23, hi: 33 }]
        : [{ label: `${this.lbl.front}低区 01-10`, lo: 1, hi: 10 }, { label: `${this.lbl.front}中区 11-20`, lo: 11, hi: 20 }, { label: `${this.lbl.front}高区 21-35`, lo: 21, hi: 35 }]
      const totalAll = this.draws.length * (this.game === 'ssq' ? 6 : 5) || 1
      return zones.map(z => {
        let total = 0
        for (let n = z.lo; n <= z.hi; n++) total += this.frontCount[n] || 0
        return { ...z, total, pct: (total / totalAll * 100).toFixed(1) }
      })
    }
  },
  methods: {
    pad(n) { return String(n).padStart(2, '0') },
    prefix() { return this.game === 'ssq' ? 'ssq/ssqGenerator/' : 'generator/' },
    gen() { return this.game === 'ssq' ? this.$store.state.ssq.ssqGenerator : this.$store.state.generator },
    lsKey() { return `trend_sel_${this.game}` },
    saveSel() {
      this.$store.commit(this.prefix() + 'SET_TREND_SEL_FRONT', this.selFront)
      this.$store.commit(this.prefix() + 'SET_TREND_SEL_BACK', this.selBack)
      try {
        localStorage.setItem(this.lsKey, JSON.stringify({ f: this.selFront, b: this.selBack }))
      } catch { /* 忽略存储异常 */ }
    },
    restoreSel() {
      // 优先取 Vuex（同一次会话内切换弹窗），其次取 localStorage（刷新后）
      let front = this.gen.trendSelFront || []
      let back = this.gen.trendSelBack || []
      if (!front.length && !back.length) {
        try {
          const s = JSON.parse(localStorage.getItem(this.lsKey) || '{}')
          front = s.f || []
          back = s.b || []
        } catch { /* 忽略 */ }
      }
      this.selFront = [...front]
      this.selBack = [...back]
      if (front.length || back.length) this.saveSel()
    },
    toggleSel(arr, n) {
      const i = arr.indexOf(n)
      if (i >= 0) arr.splice(i, 1)
      else { arr.push(n); arr.sort((a, b) => a - b) }
      this.saveSel()
    },
    clearSel() { this.selFront = []; this.selBack = []; this.saveSel() },
    selText() {
      const f = this.selFront.map(n => this.pad(n)).join(',')
      const b = this.selBack.map(n => this.pad(n)).join(',')
      return `${f}${f && b ? ' | ' : ''}${b}`
    },
    async copySel() {
      const text = this.selText()
      try {
        await navigator.clipboard.writeText(text)
      } catch {
        // 剪贴板不可用时降级为手动复制
        window.prompt('复制以下号码：', text)
      }
    },
    fillToInput() {
      if (!this.canFill) return
      const p = this.game === 'ssq' ? 'ssq/ssqGenerator/' : 'generator/'
      this.$store.commit(p + 'SET_CURRENT_FRONT', this.selFront.map(n => this.pad(n)))
      this.$store.commit(p + 'SET_CURRENT_BACK', this.selBack.map(n => this.pad(n)))
      this.$emit('close')
    },
    rangeCount(max, sets) {
      const map = {}
      for (let i = 1; i <= max; i++) map[i] = 0
      for (const s of sets) for (const n of s) if (map[n] !== undefined) map[n]++
      return map
    },
    omissions(max, sets) {
      const map = {}
      for (let i = 1; i <= max; i++) map[i] = 0
      // 注意：sets[0] 必须是最新一期（draws 保持最新在前）
      for (let i = 1; i <= max; i++) {
        let o = 0
        for (const s of sets) {
          if (s.has(i)) break
          o++
        }
        map[i] = o
      }
      return map
    },
    async load(refresh = false) {
      this.loading = true
      try {
        const r = await lotteryApi.getTrend(this.game, this.count, refresh)
        const d = r.data
        this.draws = d.draws || []
        this.frontMax = d.frontMax
        this.backMax = d.backMax
        this.cachedCount = d.cachedCount
        // 接口返回最新在前，统计（出现次数/遗漏）依赖此顺序，展示时再反转
        this.frontSets = this.draws.map(x => new Set(x.frontNumbers))
        this.backSets = this.draws.map(x => new Set(x.backNumbers))
      } finally {
        this.loading = false
      }
    }
  },
  mounted() { this.restoreSel(); this.load() }
}
</script>
