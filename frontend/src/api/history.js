import request from './request'

export default {
  save(data) {
    return request.post('/history', data)
  },
  getByIssueId(issueId) {
    return request.get(`/history/${issueId}`)
  },
  getById(id) {
    return request.get(`/history/detail/${id}`)
  },
  getStats(params) {
    return request.get('/history/stats', { params })
  },
  delete(id) {
    return request.delete(`/history/${id}`)
  }
}
