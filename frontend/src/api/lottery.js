import request from './request'

export default {
  // game: 'dlt' | 'ssq'
  getTrend(game, count = 100, refresh = false) {
    return request.get('/lottery/trend', { params: { game, count, refresh } })
  }
}
