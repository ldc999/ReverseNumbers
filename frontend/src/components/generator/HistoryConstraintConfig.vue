<template>
  <div class="config-panel panel">
    <h3>历史号码参数设置</h3>
    <div class="config-item">
      <label><input type="checkbox" v-model="enable" /> 历史数据参与随机生成</label>
    </div>
    <div v-show="enable" class="hist-config">
      <div class="config-item hist-time">
        <label>开始</label>
        <input type="datetime-local" v-model="start" class="input" style="width:180px" />
      </div>
      <div class="config-item hist-time">
        <label>结束</label>
        <input type="datetime-local" v-model="end" class="input" style="width:180px" />
      </div>
      <div class="config-item hist-time">
        <span style="font-size:12px;color:#666">限制条件 (或):</span>
        <div v-for="(c, i) in constraints" :key="i" class="constraint-row" >  
          
          <span>{{ lbl.front }}≤</span><input type="number" v-model.number="c.f" min="0" :max="lbl.frontCount" class="input-narrow" />
          <span>{{ lbl.back }}≤</span><input type="number" v-model.number="c.b" min="0" :max="lbl.backCount" class="input-narrow" />
          <button class="btn btn-sm btn-danger" @click="constraints.splice(i,1)" :disabled="constraints.length<=1">×</button>
      
        </div> 
      </div>
       <button class="btn btn-sm" @click="constraints.push({f:3,b:1})">+ 添加</button>
    </div>
  </div>
</template>

<script>
import { gameLabels } from '../../utils/gameHelper'

export default {
  name: 'HistoryConstraintConfig',
  inject: ['game'],
  data() {
    const now = new Date()
    const d30m = new Date(now.getTime() - 30 * 60000)
    const fmt = d => { const p = n => String(n).padStart(2,'0'); return `${d.getFullYear()}-${p(d.getMonth()+1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}` }
    return { temp: { start: fmt(d30m), end: fmt(now) } }
  },
  computed: {
    lbl() { return gameLabels(this.game) },
    gen() { return this.game === 'ssq' ? this.$store.state.ssq.ssqGenerator : this.$store.state.generator },
    prefix() { return this.game === 'ssq' ? 'ssq/ssqGenerator/' : 'generator/' },
    enable: { get() { return this.gen.histEnable }, set(v) { this.$store.commit(this.prefix + 'SET_HIST_ENABLE', v) } },
    start: {
      get() { return this.gen.histStart || this.temp.start },
      set(v) { this.$store.commit(this.prefix + 'SET_HIST_START', v) }
    },
    end: {
      get() { return this.gen.histEnd },
      set(v) { this.$store.commit(this.prefix + 'SET_HIST_END', v) }
    },
    constraints: {
      get() { return this.gen.histConstraints },
      set(v) { this.$store.commit(this.prefix + 'SET_HIST_CONSTRAINTS', v) }
    }
  }
}
</script>
