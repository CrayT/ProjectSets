type Person = {
    name : string
}
type name = string;
interface  product {
    price: number,
    hhh : number
}
interface product {
    weight?: any
}
interface setAge{
    (name: name, produce?: product): string;
}

const te : setAge = function(name, pro){
    console.log(name, pro)
    return name;
}
let res = te("nnn",{price: 123})
