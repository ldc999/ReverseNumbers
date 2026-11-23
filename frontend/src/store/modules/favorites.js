import favoriteNumberApi from '../../api/favoriteNumber'

export default {
  namespaced: true,
  state: {
    favoriteList: []
  },
  mutations: {
    SET_FAVORITE_LIST(state, list) {
      state.favoriteList = list
    }
  },
  actions: {
    async fetchFavorites({ commit }) {
      const res = await favoriteNumberApi.getAll()
      commit('SET_FAVORITE_LIST', res.data)
    },
    async saveFavorite({ dispatch }, { frontNumbers, backNumbers, label }) {
      await favoriteNumberApi.save({ frontNumbers, backNumbers }, label)
      await dispatch('fetchFavorites')
    },
    async deleteFavorite({ dispatch }, id) {
      await favoriteNumberApi.delete(id)
      await dispatch('fetchFavorites')
    }
  }
}
