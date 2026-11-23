import request from './request'

export default {
  save(data, label) {
    return request.post('/FavoriteNumber', data, { params: { label } })
  },
  getAll() {
    return request.get('/FavoriteNumber')
  },
  delete(id) {
    return request.delete(`/FavoriteNumber/${id}`)
  }
}
