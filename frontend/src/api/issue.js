import request from './request'

export default {
  getAll: () => request.get('/issue'),
  getById: id => request.get(`/issue/${id}`),
  create: issueNumber => request.post('/issue', { issueNumber }),
  update: (id, issueNumber) => request.put(`/issue/${id}`, { issueNumber }),
  delete: id => request.delete(`/issue/${id}`),
  setDrawResult: (id, numbers1, numbers2) => request.put(`/issue/${id}/result`, { winningNumbers1: numbers1, winningNumbers2: numbers2 })
}
