import request from './request'

export default {
  getAll() {
    return request.get('/config')
  },
  getByKey(key) {
    return request.get(`/config/${key}`)
  },
  update(key, value) {
    return request.put(`/config/${key}`, { value })
  }
}
