/**
 CreateDate:20210605
 */
const Util = {}
/**
 * 感知机哈希算法
 * @param img1
 * @param img2
 * @returns {Promise<unknown>}
 */
Util.compareImages = async function(img1, img2){
    function createMatrix (arr) {
        const length = arr.length
        const matrixWidth = Math.sqrt(length)
        const matrix = []
        for (let i = 0; i < matrixWidth; i++) {
            const _temp = arr.slice(i * matrixWidth, i * matrixWidth + matrixWidth)
            matrix.push(_temp)
        }
        return matrix
    }
    function getMatrixRange (matrix, range) {
        const rangeMatrix = []
        for (let i = 0; i < range; i++) {
            for (let j = 0; j < range; j++) {
                rangeMatrix.push(matrix[i][j])
            }
        }
        return rangeMatrix
    }

    function memoizeCosines (N, cosMap) {
        cosMap = cosMap || {}
        cosMap[N] = new Array(N * N)
        let PI_N = Math.PI / N
        for (let k = 0; k < N; k++) {
            for (let n = 0; n < N; n++) {
                cosMap[N][n + (k * N)] = Math.cos(PI_N * (n + 0.5) * k)
            }
        }
        return cosMap
    }

    function dct (signal, scale = 2) {
        let L = signal.length
        let cosMap = null
        if (!cosMap || !cosMap[L]) {
            cosMap = memoizeCosines(L, cosMap)
        }
        let coefficients = signal.map(function () { return 0 })
        return coefficients.map(function (_, ix) {
            return scale * signal.reduce(function (prev, cur, index) {
                return prev + (cur * cosMap[L][index + (ix * L)])
            }, 0)
        })
    }
    function getPHashFingerprint (imgData) {
        const dctData = dct(imgData.data)
        const dctMatrix = createMatrix(dctData)
        const rangeMatrix = getMatrixRange(dctMatrix, dctMatrix.length / 8)
        const rangeAve = rangeMatrix.reduce((pre, cur) => pre + cur, 0) / rangeMatrix.length
        return rangeMatrix.map(val => (val >= rangeAve ? 1 : 0)).join('')
    }
    function getCompareResult(p1, p2){
        let length = p1.length
        let count = 0;
        for(let i = 0; i< p1.length; i++){
            if(p1[i] === p2[i]){
                count ++
            }
        }
        return count/length
    }
    async function getImageData(url){
        const canvas = document.createElement('canvas')
        const ctx = canvas.getContext('2d')
        const img = new Image()
        const imgWidth = 16
        img.crossOrigin = "Anonymous";
        return new Promise((resolve, reject) => {
            img.onload = function () {
                canvas.width = imgWidth
                canvas.height = imgWidth
                ctx?.drawImage(img, 0, 0, imgWidth, imgWidth)
                const data = ctx?.getImageData(0, 0, imgWidth, imgWidth)
                resolve(data)
            }
            img.src = url;
        })
    }
    let p = [getImageData(img1), getImageData(img2)];
    return new Promise((resolve, reject) => {
        Promise.all(p).then((res) => {
            let p1 = getPHashFingerprint(res[0])
            let p2 = getPHashFingerprint(res[1])
            let ratio = getCompareResult(p1, p2)
            resolve(ratio)
        })
    })
}
Util.UUID8Bit = function () {
    var chars = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
        "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x",
        "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A",
        "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
        "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"];

    var uuid = "";
    for (var i = 0; i < 8; ++i) {
        var index = Math.min(Math.floor(Math.random() * 62), 61);
        uuid += chars[index];
    }
    return uuid;
};
Util.delay = (timeout = 0) => (
    new Promise(resolve => {
        setTimeout(resolve, timeout);
    })
);
Util.getUrlParameter = function (name) {
    if (name) {
        var pattern = "(^|&)" + name + "=([^&]*)(&|$)";
        var flags = "i"; // 大小写不记
        var reg = new RegExp(pattern, flags); //构造一个含有目标参数的正则表达式对象
        var result = window.location.search.substr(1).match(reg);  //匹配目标参数
        if (result) return decodeURIComponent(result[2]);
        return null; //返回参数值
    }
};
export {Util}