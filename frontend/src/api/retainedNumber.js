import request from './request'

export default {
  save(issueId, data) {
    return request.post(`/RetainedNumber/${issueId}`, data)
  },
  getByIssueId(issueId) {
    return request.get(`/RetainedNumber/${issueId}`)
  },
  delete(id) {
    return request.delete(`/RetainedNumber/${id}`)
  }
}
