import request from './request'

export default {
  save(issueId, data) { return request.post(`/Ticket/${issueId}`, data) },
  getByIssueId(issueId) { return request.get(`/Ticket/${issueId}`) },
  updateConstraints(id, constraints) { return request.put(`/Ticket/${id}/constraints`, constraints) },
  getAnalysis() { return request.get('/Ticket/analysis') },
  addToPlayer(id) { return request.post(`/Ticket/${id}/to-player`) },
  delete(id) { return request.delete(`/Ticket/${id}`) }
}
