/**
 CreateDate:20210605
 */
const Fetcher = {}
Fetcher.Config={
    GetImageListHost : "//webapi-test.123kanfang.com:5027",

}
Fetcher.API ={
    ImageAPI : "/FloorPlanPath/GetFloorPlan"
}
Fetcher.getData = async function(path){
    let response = await fetch(path || `https://vrhouse-web.oss-cn-shanghai.aliyuncs.com/31test-403/Config.js`);
    return new Promise((resolve, reject) => {
        response.text().then( res => {
            if (typeof res === 'string') {
                resolve(res);
            } else {
                reject(res);
            }
        }).catch(e => {
            reject(e);
        })
    })
}
Fetcher.getImagePath = async function(path){
    let packageId = path.PackageId;
    let bucket = path.Bucket;
    let response = await fetch(`${Fetcher.Config.GetImageListHost}${Fetcher.API.ImageAPI}?bucket=${bucket}&packageId=${packageId}`,{
        method: "GET",
        mode:"cors"
    });
    return new Promise((resolve, reject) => {
        response.json().then( res => {
            resolve(res);
        }).catch(e => {
            reject(e);
        })
    })
}
export {Fetcher}