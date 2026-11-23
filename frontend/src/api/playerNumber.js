import request from './request'

export default {
  getByIssueId(issueId) {
    return request.get(`/issue/${issueId}/player-numbers`)
  },
  add(issueId, frontNumbers, backNumbers) {
    return request.post(`/issue/${issueId}/player-numbers`, { frontNumbers, backNumbers })
  },
  batchAdd(issueId, sets) {
    return request.post(`/issue/${issueId}/player-numbers/batch`, { sets })
  },
  delete(issueId, id) {
    return request.delete(`/issue/${issueId}/player-numbers/${id}`)
  },
  clearAll(issueId) {
    return request.delete(`/issue/${issueId}/player-numbers`)
  }
}
