<template>
  <div class="history-panel panel">
    <h3 class="panel-title" @click="collapsed = !collapsed"><span class="toggle-arrow">{{ collapsed ? '▶' : '▼' }}</span> 守号 (全局收藏)</h3>
    <div v-show="!collapsed">
      <div v-if="favoriteList.length === 0" class="hint">暂无收藏号码</div>
      <div v-else class="history-list">
        <div v-for="item in favoriteList" :key="item.id" class="history-item">
          <div class="history-numbers-row"><span v-if="item.label" class="fav-label">{{ item.label }}</span><span class="history-numbers">前区 {{ fmt(item.frontNumbers) }}  |  后区 {{ fmt(item.backNumbers) }}</span></div>
          <div class="history-meta-row"><span class="history-time">{{ formatTime(item.createdAt) }}</span><div class="history-actions"><button class="btn btn-sm btn-primary" @click="restore(item)">回填</button><button class="btn btn-sm btn-danger" @click="remove(item.id)">删除</button></div></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ns } from '../../utils/gameHelper'

export default {
  name: 'FavoriteNumbersPanel',
  inject: ['game'],
  data() { return { collapsed: false } },
  computed: {
    favoriteList() { return this.game === 'ssq' ? this.$store.state.ssq.ssqFavorites.favoriteList : this.$store.state.favorites.favoriteList }
  },
  methods: {
    dispatch(p, d) { return this.$store.dispatch(ns(this.game, p), d) },
    commit(p, d) { this.$store.commit(ns(this.game, p), d) },
    fmt(arr) { return arr.map(n => String(n).padStart(2, '0')).join(', ') },
    formatTime(d) { return d ? new Date(d).toLocaleString('zh-CN') : '' },
    restore(item) { this.commit('generator/SET_CURRENT_FRONT', item.frontNumbers.map(n => String(n).padStart(2, '0'))); this.commit('generator/SET_CURRENT_BACK', item.backNumbers.map(n => String(n).padStart(2, '0'))) },
    async remove(id) { if (!confirm('确定删除？')) return; await this.dispatch('favorites/deleteFavorite', id) }
  },
  mounted() { this.dispatch('favorites/fetchFavorites') }
}
</script>
