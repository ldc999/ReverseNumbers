import request from './request'

const base = '/ssq'

export const ssqIssueApi = {
  getAll: () => request.get(`${base}/issue`),
  getById: id => request.get(`${base}/issue/${id}`),
  create: issueNumber => request.post(`${base}/issue`, { issueNumber }),
  update: (id, issueNumber) => request.put(`${base}/issue/${id}`, { issueNumber }),
  delete: id => request.delete(`${base}/issue/${id}`),
  setDrawResult: (id, n1, n2) => request.put(`${base}/issue/${id}/result`, { winningNumbers1: n1, winningNumbers2: n2 })
}

export const ssqPlayerNumberApi = {
  getByIssueId: issueId => request.get(`${base}/issue/${issueId}/player-numbers`),
  add: (issueId, front, back) => request.post(`${base}/issue/${issueId}/player-numbers`, { frontNumbers: front, backNumbers: back }),
  batchAdd: (issueId, sets) => request.post(`${base}/issue/${issueId}/player-numbers/batch`, { sets }),
  delete: (issueId, id) => request.delete(`${base}/issue/${issueId}/player-numbers/${id}`),
  clearAll: issueId => request.delete(`${base}/issue/${issueId}/player-numbers`)
}

export const ssqGenerateApi = {
  generateRandom: params => request.post(`${base}/generate/random`, params),
  highlight: params => request.post(`${base}/generate/highlight`, params)
}

export const ssqHistoryApi = {
  save: data => request.post(`${base}/history`, data),
  getByIssueId: issueId => request.get(`${base}/history/${issueId}`),
  getById: id => request.get(`${base}/history/detail/${id}`),
  getStats: params => request.get(`${base}/history/stats`, { params }),
  delete: id => request.delete(`${base}/history/${id}`)
}

export const ssqConfigApi = {
  getAll: () => request.get(`${base}/config`),
  getByKey: key => request.get(`${base}/config/${key}`),
  update: (key, value) => request.put(`${base}/config/${key}`, { value })
}

export const ssqRetainedNumberApi = {
  save: (issueId, data) => request.post(`${base}/retained-number/${issueId}`, data),
  getByIssueId: issueId => request.get(`${base}/retained-number/${issueId}`),
  delete: id => request.delete(`${base}/retained-number/${id}`)
}

export const ssqFavoriteNumberApi = {
  save: (data, label) => request.post(`${base}/favorite-number`, data, { params: { label } }),
  getAll: () => request.get(`${base}/favorite-number`),
  delete: id => request.delete(`${base}/favorite-number/${id}`)
}

export const ssqTicketApi = {
  save: (issueId, data) => request.post(`${base}/Ticket/${issueId}`, data),
  getByIssueId: issueId => request.get(`${base}/Ticket/${issueId}`),
  updateConstraints: (id, constraints) => request.put(`${base}/Ticket/${id}/constraints`, constraints),
  getAnalysis: () => request.get(`${base}/Ticket/analysis`),
  addToPlayer: id => request.post(`${base}/Ticket/${id}/to-player`),
  delete: id => request.delete(`${base}/Ticket/${id}`)
}
