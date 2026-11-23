<template>
  <div class="player-number-input">
    <div class="section-header">
      <h3>玩家号码输入 (共 {{ viewMode === 'single' ? sortedPlayerSets.length : ticketList.length }} 条)</h3>
      <div class="header-actions">
        <span class="view-toggle">
          <button class="btn btn-sm" :class="{ 'btn-primary': viewMode === 'single' }"
            @click="viewMode = 'single'">单注</button>
          <button class="btn btn-sm" :class="{ 'btn-primary': viewMode === 'ticket' }"
            @click="viewMode = 'ticket'">票号</button>
        </span>
        <button class="btn btn-primary btn-sm" @click="showAddRow = true" :disabled="!issueId"
          v-if="viewMode === 'single'">添加一注</button>
        <button class="btn btn-sm" @click="showBatch = true" :disabled="!issueId"
          v-if="viewMode === 'single'">批量导入</button>
        <button class="btn btn-sm" @click="showTicket = true" :disabled="!issueId">票号输入</button>
        <button class="btn btn-danger btn-sm" @click="clearAllSets"
          :disabled="!issueId || sortedPlayerSets.length === 0" v-if="viewMode === 'single'">清空全部</button>
      </div>
    </div>
    <div v-if="!issueId" class="hint">请先选择期号</div>
    <TicketPanel v-if="showTicket" @saved="showTicket = false; fetch(); fetchTickets()" @close="showTicket = false" />
    <div v-if="showAddRow" class="add-row">
      <NumberSquareGroup :editable="true" :frontNumbers.sync="newFront" :backNumbers.sync="newBack"
        :frontLabel="lbl.front" :backLabel="lbl.back" />
      <div class="add-row-actions">
        <button class="btn btn-primary btn-sm" @click="addOne">确认添加</button>
        <button class="btn btn-sm" @click="showAddRow = false">取消</button>
      </div>
    </div>
    <div v-if="showBatch" class="batch-section">
      <textarea v-model="batchText" class="textarea" rows="6"
        :placeholder="'每行一注，格式：01,05,12,23,35|03,09&#10;' + lbl.front + lbl.frontCount + '个号码(逗号分隔)|' + lbl.back + lbl.backCount + '个号码(逗号分隔)'"></textarea>
      <div class="add-row-actions"><button class="btn btn-primary btn-sm" @click="batchAdd">批量导入</button><button
          class="btn btn-sm" @click="showBatch = false">取消</button></div>
    </div>
    <!-- Single bet view -->
    <div class="player-set-list" v-if="viewMode === 'single' && sortedPlayerSets.length > 0">
      <PlayerNumberSet v-for="(set, idx) in pagedSets" :key="set.id" :set="set" :index="(page - 1) * pageSize + idx"
        :highlightData="highlightData" @delete="removeOne(set.id)" />
    </div>
    <div v-if="viewMode === 'single' && totalPages > 1" class="pagination">
      <button class="btn btn-sm" :disabled="page <= 1" @click="page--">上一页</button>
      <span>{{ page }} / {{ totalPages }} (共{{ sortedPlayerSets.length }}注)</span>
      <button class="btn btn-sm" :disabled="page >= totalPages" @click="page++">下一页</button>
    </div>
    <div v-else-if="viewMode === 'single' && issueId" class="hint">暂无号码，请添加或导入</div>

    <!-- Ticket view -->
    <div class="player-set-list" v-if="viewMode === 'ticket'">
      <!-- Cross-issue analysis -->
      <div class="ticket-draw-analysis" v-if="crossAnalysis.length">
        <strong @click="showCross = !showCross" style="cursor:pointer">
          <span class="toggle-arrow">{{ showCross ? '▼' : '▶' }}</span> 跨期号票号分析 (共{{ crossAnalysis.length }}期)
        </strong>
        <table v-show="showCross" class="analysis-table">
          <thead>
            <tr>
              <th>期号</th>
              <th>票号</th>
              <th>类型</th>
              <th>前区数</th>
              <th>前中</th>
              <th>后区数</th>
              <th>后中</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in crossAnalysis" :key="r.ticketId">
              <td>{{ r.issueNumber }}</td>
              <td>#{{ r.ticketId }}</td>
              <td>{{ r.ticketType === 'multi' ? '多注' : '复式' }}</td>
              <td>{{ r.frontPoolCount }}</td>
              <td :class="{ hit: r.frontMatchCount > 0 }">{{ r.frontMatchCount }}</td>
              <td>{{ r.backPoolCount }}</td>
              <td :class="{ hit: r.backMatchCount > 0 }">{{ r.backMatchCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!ticketList.length && issueId" class="hint">暂无票号</div>
      <!-- Draw analysis -->
      <div v-if="ticketList.length > 0">
        <div v-if="drawAnalysis.length" class="ticket-draw-analysis">
          <strong>开奖分析 ({{ lbl.front }}池/{{ lbl.back }}池中开奖号出现情况):</strong>
          <div class="analysis-grid">
            <span v-for="item in drawAnalysis" :key="item.k" class="analysis-item">
              {{ item.k }}: <em>{{ item.c }}张</em>
            </span>
          </div>
        </div>
        <div v-for="ticket in ticketList" :key="ticket.id" class="ticket-view-card">
          <div class="ticket-view-header">
            <span>票号 #{{ ticket.id }}</span>
            <span class="ticket-type-tag" :class="ticket.ticketType">{{ ticket.ticketType === 'multi' ? '多注' : '复式'
              }}</span>
            <span class="ticket-player-tag" :class="{ added: ticket.addToPlayer }">{{ ticket.addToPlayer ? '已入玩家' :
              '未入玩家' }}</span>
            <button v-if="!ticket.addToPlayer" class="btn btn-sm btn-success"
              @click="addTicketToPlayer(ticket)">转入玩家</button>
            <span class="ticket-hit-info" v-if="highlightData">{{ ticketHit(ticket) }}</span>

            <button class="btn btn-sm btn-danger" @click="deleteTicket(ticket.id)">删除</button>
          </div>
          <div class="ticket-view-header">
            <span class="ticket-pool-info">{{ lbl.front }}池: {{ fmtPool(ticket.frontPool) }} | {{ lbl.back }}池: {{
              fmtPool(ticket.backPool) }}</span> 
          </div>
           <div class="ticket-view-header">
            <span class="ticket-pool-info">未选{{ lbl.front }}池: {{ noNumbersFront(ticket) }} | 未选{{ lbl.back }}池: {{
           noNumbersBack(ticket)  }}</span> 
          </div>
          <!-- Multi bet lines -->
          <div v-if="ticket.ticketType === 'multi' && ticket.bets" class="ticket-bets">
            <div v-for="(bet, i) in ticket.bets" :key="i" class="ticket-bet-row">
              <span class="set-index">{{ i + 1 }}</span>
              <span class="ticket-bet-nums">{{ bet.frontNumbers }} | {{ bet.backNumbers }}</span>
            </div>
          </div>
          <div class="ticket-pool-size">
            {{ lbl.front }}{{ (ticket.frontPool || []).length }}个{{ drawMatchInfo(ticket) }}
            <span class="zone-break">(低{{ zoneCount(ticket.frontPool, 0) }}/中{{ zoneCount(ticket.frontPool, 1) }}/高{{
              zoneCount(ticket.frontPool, 2) }})</span>
            + {{ lbl.back }}{{ (ticket.backPool || []).length }}个{{ drawMatchInfoBack(ticket) }}
          </div>
          <div class="ticket-constraints-edit">
            <span class="adv-label">限制条件(或):</span>
            <div v-for="(c, i) in ticket.constraints" :key="i" class="constraint-row">
              <span>{{ lbl.front }}</span>
              <input v-model.number="c.fMin" type="number" min="0" :max="lbl.frontCount" class="input-narrow" />
              <span>≤</span><span>{{ lbl.front }}</span><span>≤</span>
              <input v-model.number="c.f" type="number" min="0" :max="lbl.frontCount" class="input-narrow" />
              <span>{{ lbl.back }}</span>
              <input v-model.number="c.bMin" type="number" min="0" :max="lbl.backCount" class="input-narrow" />
              <span>≤</span><span>{{ lbl.back }}</span><span>≤</span>
              <input v-model.number="c.b" type="number" min="0" :max="lbl.backCount" class="input-narrow" />
              <button class="btn btn-sm btn-danger" @click="ticket.constraints.splice(i, 1)">×</button>
            </div>
            <button class="btn btn-sm" @click="ticket.constraints.push({ fMin: 0, f: 3, bMin: 0, b: 0 })">+ 添加</button>
            <button class="btn btn-sm btn-primary" @click="saveConstraints(ticket)">保存条件</button>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="viewMode === 'ticket' && issueId" class="hint">暂无票号</div>
  </div>
</template>

<script>
import NumberSquareGroup from '../common/NumberSquareGroup.vue'
import PlayerNumberSet from './PlayerNumberSet.vue'
import TicketPanel from './TicketPanel.vue'
import ticketApi from '../../api/ticket'
import { ssqTicketApi } from '../../api/ssq'
import { ns, gameLabels, emptyNumbers } from '../../utils/gameHelper'

export default {
  name: 'PlayerNumberInput',
  components: { NumberSquareGroup, PlayerNumberSet, TicketPanel },
  inject: ['game'],
  props: { highlightData: { type: Object, default: null } },
  data() {
    return {
      showAddRow: false, showBatch: false, showTicket: false,
      viewMode: 'single',
      page: 1, pageSize: 2000,
      crossAnalysis: [], showCross: false,
      newFront: emptyNumbers(this.game).front,
      newBack: emptyNumbers(this.game).back,
      batchText: ''
    }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    issueId() { return this.$store.getters[ns(this.game, 'issue/currentIssueId')] },
    playerSets() { return this.game === 'ssq' ? this.$store.state.ssq.ssqPlayerNumbers.playerSets : this.$store.state.playerNumbers.playerSets },
    ticketList() { return this.game === 'ssq' ? this.$store.state.ssq.ssqTickets.ticketList : this.$store.state.tickets.ticketList },
    drawAnalysis() {
      const ci = this.game === 'ssq' ? this.$store.state.ssq.ssqIssue.currentIssue : this.$store.state.issue.currentIssue
      if (!ci) return []
      const wf = (ci.winningFrontNumbers || ci.winningRedNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))
      const wb = (ci.winningBackNumbers || ci.winningBlueNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))
      if (!wf.length && !wb.length) return []
      const map = {}
      for (const t of this.ticketList) {
        const f = (t.frontPool || []).filter(n => wf.includes(n)).length
        const b = (t.backPool || []).filter(n => wb.includes(n)).length
        const k = `${f}+${b}`
        map[k] = (map[k] || 0) + 1
      }
      return Object.entries(map).map(([k, c]) => ({ k, c }))
    },
    sortedPlayerSets() {
      if (!this.highlightData) return this.playerSets
      const inputFront = new Set(this.highlightData.frontNumbers || [])
      const inputBack = new Set(this.highlightData.backNumbers || [])
      return [...this.playerSets].sort((a, b) => {
        const countA = this.matchCount(a, inputFront, inputBack)
        const countB = this.matchCount(b, inputFront, inputBack)
        return countB - countA
      })
    },
    totalPages() { return Math.ceil(this.sortedPlayerSets.length / this.pageSize) },
    pagedSets() {
      const start = (this.page - 1) * this.pageSize
      return this.sortedPlayerSets.slice(start, start + this.pageSize)
    }
  },
  watch: {
    issueId(val) { if (val) { this.fetch(); this.fetchTickets() } }
  },
  methods: {
    dispatch(p, d) { return this.$store.dispatch(ns(this.game, p), d) },
    fetch() { return this.dispatch('playerNumbers/fetchPlayerNumbers', this.issueId) },
    async addOne() {
      const f = this.newFront.map(n => String(n).padStart(2, '0')).join(',')
      const b = this.newBack.map(n => String(n).padStart(2, '0')).join(',')
      await this.dispatch('playerNumbers/addPlayerSet', { issueId: this.issueId, frontNumbers: f, backNumbers: b })
      this.newFront = emptyNumbers(this.game).front
      this.newBack = emptyNumbers(this.game).back
      this.showAddRow = false
    },
    async batchAdd() {
      const lines = this.batchText.trim().split('\n').filter(Boolean)
      const sets = lines.map(l => { const p = l.trim().split('|'); return p.length === 2 ? { frontNumbers: p[0].trim(), backNumbers: p[1].trim() } : null }).filter(Boolean)
      if (sets.length > 0) { await this.dispatch('playerNumbers/batchAddPlayerSets', { issueId: this.issueId, sets }); this.batchText = ''; this.showBatch = false }
    },
    matchCount(set, inputFront, inputBack) {
      const f = (set.frontNumbers || set.redNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))
      const b = (set.backNumbers || set.blueNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))
      return f.filter(n => inputFront.has(n)).length + b.filter(n => inputBack.has(n)).length
    },
    async removeOne(id) { if (!confirm('确定删除该注号码吗？')) return; await this.dispatch('playerNumbers/deletePlayerSet', { issueId: this.issueId, id }) },
    async clearAllSets() { if (confirm('确定清空所有玩家号码吗？')) await this.dispatch('playerNumbers/clearAll', this.issueId) },
    winFront() { const ci = this.game === 'ssq' ? this.$store.state.ssq.ssqIssue.currentIssue : this.$store.state.issue.currentIssue; return (ci && (ci.winningFrontNumbers || ci.winningRedNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))) || [] },
    winBack() { const ci = this.game === 'ssq' ? this.$store.state.ssq.ssqIssue.currentIssue : this.$store.state.issue.currentIssue; return (ci && (ci.winningBackNumbers || ci.winningBlueNumbers || '').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n))) || [] },
    drawMatchInfo(ticket) { const wf = this.winFront; if (!wf.length) return ''; const m = (ticket.frontPool || []).filter(n => wf.includes(n)).length; return m > 0 ? ` (中${m})` : '' },
    drawMatchInfoBack(ticket) { const wb = this.winBack; if (!wb.length) return ''; const m = (ticket.backPool || []).filter(n => wb.includes(n)).length; return m > 0 ? ` (中${m})` : '' },
    zoneCount(pool, zi) {
      if (!pool || !pool.length) return 0
      const isSsq = this.game === 'ssq'
      const zones = isSsq ? [[1, 11], [12, 22], [23, 33]] : [[1, 10], [11, 20], [21, 35]]
      const [lo, hi] = zones[zi]
      return pool.filter(n => n >= lo && n <= hi).length
    },
    async fetchTickets() { await this.dispatch('tickets/fetchTickets', this.issueId); this.loadCrossAnalysis() },
    async loadCrossAnalysis() {
      try {
        const api = this.game === 'ssq' ? ssqTicketApi : ticketApi
        const r = await api.getAnalysis(); this.crossAnalysis = r.data || []
      } catch { this.crossAnalysis = [] }
    },
    fmtPool(arr) { return (arr || []).map(n => String(n).padStart(2, '0')).join(',') },
    ticketHit(ticket) {
      if (!this.highlightData) return ''
      const fSet = new Set(this.highlightData.frontNumbers || [])
      const bSet = new Set(this.highlightData.backNumbers || [])
      const fNums = (ticket.frontPool || []).filter(n => fSet.has(n)).sort((a, b) => a - b)
      const bNums = (ticket.backPool || []).filter(n => bSet.has(n)).sort((a, b) => a - b)
      const fStr = fNums.map(n => String(n).padStart(2, '0')).join(',')
      const bStr = bNums.map(n => String(n).padStart(2, '0')).join(',')
      return `${fNums.length}+${bNums.length} (${fStr || '-'} - ${bStr || '-'})`
    },

    noNumbersFront(ticket) {
      const allFrontCount = this.game === 'ssq' ? 33 : 35
      const pool = ticket.frontPool || []
      const noFront = []
      for (let i = 1; i <= allFrontCount; i++) {
        if (!pool.includes(i)) noFront.push(i)
      }
      return noFront.join(',')
    },
    noNumbersBack(ticket) {
      const allBackCount = this.game === 'ssq' ? 16 : 12
      const pool = ticket.backPool || []
      const noBack = []
      for (let i = 1; i <= allBackCount; i++) {
        if (!pool.includes(i)) noBack.push(i)
      }
      return noBack.join(',')
    },

    async saveConstraints(ticket) {
      await this.dispatch('tickets/updateConstraints', { id: ticket.id, constraints: ticket.constraints, issueId: this.issueId })
    },
    async addTicketToPlayer(ticket) {
      const api = this.game === 'ssq' ? ssqTicketApi : ticketApi
      await api.addToPlayer(ticket.id)
      await this.fetch()
      await this.fetchTickets()
    },
    async deleteTicket(id) {
      if (!confirm('确定删除该票号吗？其关联的玩家号码也会被删除。')) return
      await this.dispatch('tickets/deleteTicket', { id, issueId: this.issueId })
      await this.fetch()
    },
  }
}
</script>
