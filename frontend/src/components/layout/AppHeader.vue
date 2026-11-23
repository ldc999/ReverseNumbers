<template>
  <div class="app-header">
    <h1 class="app-title">彩票选号助手</h1>
    <div class="header-nav">
      <router-link to="/dlt" class="nav-link" active-class="nav-active">大乐透</router-link>
      <router-link to="/ssq" class="nav-link" active-class="nav-active">双色球</router-link>
      <span v-if="currentIssueNumber" class="current-issue">当前期号：{{ currentIssueNumber }}</span>
    </div>
  </div>
</template>

<script>
export default {
  name: 'AppHeader',
  inject: {
    game: { default: 'dlt' }
  },
  computed: {
    currentIssueNumber() {
      try {
        if (this.game === 'ssq') return this.$store.state.ssq.ssqIssue.currentIssue?.issueNumber || ''
        return this.$store.state.issue.currentIssue?.issueNumber || ''
      } catch { return '' }
    }
  }
}
</script>
