<template>
  <div id="container"></div>
</template>

<script>
export default {
  name: "CanvasPanel",
  data(){
    return {
      points: [{"x":37.96009508554653,"y":-70,"z":-67.8506936948386},{"x":-28.105114688252797,"y":-70,"z":-69.54439371177206},{"x":-28.682708775481842,"y":-70,"z":-47.01448214541303},{"x":-53.724480852373745,"y":-33.7,"z":-47.65647221458609},{"x":-53.724480852373745,"y":-33.7,"z":-47.65647221458609},{"x":-54.33609009511201,"y":-33.7,"z":-23.799749203299612},{"x":-54.33609009511201,"y":-33.7,"z":-23.799749203299612},{"x":-29.294318018198283,"y":-70,"z":-23.157759134126778},{"x":-29.313556195533238,"y":-70,"z":-22.407345576127014},{"x":-153.3810555934724,"y":-70,"z":-25.588038191198862},{"x":-153.5094023319698,"y":-70,"z":-20.58168951030258},{"x":-154.4645770954403,"y":-70,"z":16.676272574045015},{"x":-154.59251975241023,"y":-70,"z":21.666859475529918},{"x":-23.44502593969537,"y":-70,"z":25.02905988516909},{"x":-27.658088565226535,"y":-70,"z":189.36571422612224},{"x":27.2450083778549,"y":-70,"z":190.77325414584016},{"x":33.74379421991489,"y":-70,"z":33.91575998899748},{"x":35.89636714866447,"y":-70,"z":-18.039667348158005},{"x":37.96009508554653,"y":-70,"z":-67.8506936948386}],
    }
  },
  methods:{
    resolvePos(pos){
      let bn = pos.split("{")
      bn.splice(0,1);
      let newArray = []
      bn.forEach(b => {
        newArray.push(JSON.parse("{" + b.split("}")[0] + "}"))
      })
      console.log(newArray)
      return newArray
    },
    initThree(){
      let height = window.innerHeight;
      let width = window.innerWidth * .8;
      this.renderer = new THREE.WebGLRenderer({
        antialias:true
      });
      this.renderer.setSize(width, height);
      document.getElementById('container').appendChild( this.renderer.domElement );
    },
    initScene(){
      let obj = new THREE.Object3D();
      obj.name='obj'
      this.scene = new THREE.Scene();
      this.scene.add(obj)
    },
    initGrid(){
      let size = 10000;
      let divisions = 100;
      let gridHelper = new THREE.GridHelper( size, divisions );
      gridHelper.position.setY(-1)
      this.scene.add( gridHelper );
    },
    getMaxAndMin(points){
      let x=[], y = [], z = []
      points.forEach(p =>{
        if(p.hasOwnProperty('x')){
          x.push(p.x)
          y.push(p.y)
          z.push(p.z)
        }else{
          x.push(p.X)
          y.push(p.Y)
          z.push(p.Z)
        }
      })
      let centerX = (Math.max.apply(null,x) + Math.min.apply(null,x)) / 2;
      let centerY = Math.max.apply(null,y) + Math.min.apply(null,y);
      let centerZ = (Math.max.apply(null,z) + Math.min.apply(null,z)) / 2;
      let pos = new THREE.Vector3(centerX, centerY, centerZ)
      return pos
    },
    getCenter(points){
      let x=0, y = 0, z = 0
      points.forEach(p =>{
        if(p.hasOwnProperty('x')){
          if(x > p.x) x = p.x
          if(y > p.y) y = 0
          if(z > p.z) z = p.z
        }else{
          x += p.X
          y += p.Y
          z += p.Z
        }
      })
      return new THREE.Vector3(x, y, z)
    },
    initAsix(){
      let worldAxis = new THREE.AxesHelper(1000);
      worldAxis.position.copy(new THREE.Vector3())
      this.scene.add(worldAxis)

      //两条坐标轴：
      let material = new THREE.LineBasicMaterial({vertexColors: THREE.VertexColors}); //括号内参数表示线条的颜色会根据顶点来计算。
      let geometry1 = new THREE.Geometry();
      let geometry2 = new THREE.Geometry();
      let color1 = new THREE.Color(0x444444);
      let color2 = new THREE.Color(0xFF0000);
      geometry1.vertices.push(new THREE.Vector3(-5000,0,0), new THREE.Vector3(5000,0,0)); //摄像机坐标顺序为：(x，y，z)，x向右，y向上，z向里
      geometry1.colors.push(color2, color1); //定义顶点两个颜色，渐变色。
      geometry2.vertices.push(new THREE.Vector3(0,0,-5000), new THREE.Vector3(0,0,5000)); //摄像机坐标顺序为：(x，y，z)，x向右，y向上，z向里
      geometry2.colors.push(color2, color1); //定义顶点两个颜色，渐变色。
      let line1 = new THREE.Line(geometry1, material, THREE.LineSegments);
      let line2 = new THREE.Line(geometry2, material, THREE.LineSegments);
      this.scene.add(line1)
      this.scene.add(line2)
    },
    initCamera(points){
      let camera = new THREE.OrthographicCamera(-window.innerWidth*.8 / 2, window.innerWidth*.8 / 2, window.innerHeight / 2, -window.innerHeight / 2, 0.1, 10000);
      try{
        camera.position.copy(new THREE.Vector3(0, 200, 0));
      } catch(e){
          camera.up.x = 0;
          camera.up.y = 0;
          camera.up.z = 1;
        }
      this.camera = camera;
      this.scene.add(camera);
    },
    conPoints(points){
      let pp= []
      points.forEach(p =>{
        if(p.hasOwnProperty('x')){
          pp.push(p.x, p.y, p.z)
        }else{
          pp.push(p.X, p.Y, p.Z)
        }
      })
      return pp
    },
    orbitControls(){
      let controls = new THREE.OrbitControls(this.camera, this.renderer.domElement );
      controls.target.set(0, 0, 0);
      controls.enableZoom = true;
      controls.enablePan = true;
      controls.enableDamping = true;
      controls.dampingFactor = 0.5;  // 缓冲效果下的加速度
      controls.rotateSpeed = -1;
      controls.update();
      this.control = controls;
    },
    initObject(points){
      try{
        let obj = this.scene.getObjectByName('obj')
        this.scene.remove(obj)
        obj = new THREE.Object3D();
        obj.name='obj'
        this.scene.add(obj)
        let pointArr = this.conPoints(points)
        let geometry = new THREE.LineGeometry();
        // 几何体传入顶点坐标
        let vertices = []
        pointArr.forEach(p =>{
          vertices.push( p);
        })
        geometry.setPositions(vertices)
        let material  = new THREE.LineMaterial( {
          color: 0xdd2222,
          linewidth: 0.003,
        } );
        let line = new THREE.Line2();
        line.geometry = geometry;
        line.material = material;
        line.name = 'line'
        obj.add(line);

        points.forEach((p, index) =>{
          let point = new THREE.Points();
          console.log(p)
          // point.position.copy(p)
          point.material = new THREE.PointsMaterial({
            size: 10,
            sizeAttenuation: true,
            vertexColors: THREE.VertexColors,
          });
          let geometry = new THREE.BufferGeometry();
          let positions = [];
          positions.push(p.x, p.y, p.z);
          // 颜色
          let vx =  0.1;
          let vy = 0.8;
          let vz = 0.4;
          let color = new THREE.Color();
          color.setRGB(vx, vy, vz);
          let colors = []
          colors.push(color.r, color.g, color.b);
          geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
          geometry.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3));
          point.geometry = geometry
          point.name = 'point'
          obj.add(point)
        })
      } catch(e){
        console.log(e)
      }
    },
    animation() {
      this.renderer.render(this.scene, this.camera);
      requestAnimationFrame(this.animation);
    },
    threeStart(points, reRender){
      if(typeof points == 'string'){
        let newPos = this.resolvePos(points)
        points = newPos
      } else if(typeof points == 'Array'){
        points = points
      }
      if(!reRender){
        this.initThree();
        this.initScene();
        this.initCamera(points);
        this.initGrid();
        this.orbitControls(points);
        this.initAsix();
        this.initObject(points)
      } else {
        this.initObject(points)
      }
      this.animation();
    },
    reRenderCanvas(){
      this.threeStart(this.$store.state.points, true)
    }
  },
  mounted() {
    this.threeStart(this.$store.state.points || this.points)
  },
}
</script>
<style scoped>
body {
  margin: 0;
}
#container {
  width: 80%;
  height: 100%;
  left:20%;
  position: absolute
}
</style>
