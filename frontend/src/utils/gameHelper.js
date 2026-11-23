// Module name mapping: DLT → SSQ
const DLT_TO_SSQ = {
  issue: 'ssqIssue',
  playerNumbers: 'ssqPlayerNumbers',
  generator: 'ssqGenerator',
  history: 'ssqHistory',
  retainedNumbers: 'ssqRetained',
  favorites: 'ssqFavorites',
  tickets: 'ssqTickets'
}

// Resolve Vuex namespace path.
// Components always use the DLT module name.
// When game='ssq', the path is remapped and prefixed with 'ssq/'.
export function ns(game, modulePath) {
  if (game === 'ssq') {
    const parts = modulePath.split('/')
    const mod = DLT_TO_SSQ[parts[0]] || parts[0]
    return `ssq/${mod}/${parts.slice(1).join('/')}`
  }
  return modulePath
}
// Game-specific labels and counts
export function gameLabels(game) {
  return game === 'ssq'
    ? { front: '红球', back: '蓝球', frontCount: 6, backCount: 1 }
    : { front: '前区', back: '后区', frontCount: 5, backCount: 2 }
}

export function noNombersMethod(game, front, back) {
  var allFronCount = game === 'ssq' ? 33 : 35
  var allBackCount = game === 'ssq' ? 16 : 12
  var allFron = new Array();
  for (var i = 0; i < allFronCount; i++) {
    allFron.push(i+1);
  }

  var allBack = new Array();
  for (var i = 0; i < allBackCount; i++) {
    allBack.push(i+1);
  }
 
  return {
    nofront: allFron.filter(element => !front.includes(element)),
    noback: allBack.filter(element => !back.includes(element))
  }
}


export function emptyNumbers(game) {
  const cfg = gameLabels(game)
  var front = new Array(cfg.frontCount).fill('')
  var back = new Array(cfg.backCount).fill('')
  var noNombers = noNombersMethod(game, front, back)
  var nofront = noNombers.nofront
  var noback = noNombers.noback
  return {
    front ,
    back,
    nofront,
    noback
  }
}
