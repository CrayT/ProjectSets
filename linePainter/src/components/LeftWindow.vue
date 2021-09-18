<template>
  <div>
    <button id="reRender" @click="reRender">重新渲染</button>
    <div class="posHint">Please input positions:</div>
    <textarea id="inputArea" v-model="value"></textarea>

  </div>
</template>

<script>
import initial from "../mixin/initial";

export default {
  name: "LeftWindow",
  data(){
    return {
      value:'{"x":-200,"y":-70,"z":-100},{"x":-200,"y":-70,"z":100},{"x":100,"y":-70,"z":100},{"x":300,"y":-70,"z":-300},{"x":-200,"y":-70,"z":-100}',
    }
  },
  mixins : [initial],
  methods:{
    reRender(){
      console.log(this.value)
      this.$store.state.points = this.value
      this.$emit("reRend")
    }
  },
  mounted() {
    console.log("mounted")
    this.$store.state.points = this.value;
  },
  directives:{
    focus:{
      inserted:function(el){
        el.focus();
      }
    }
  }
}
</script>

<style scoped>
#inputArea{
  z-index: 1;
  position: absolute;
  height: 80%;
  width: 19%;
  top:100px;
  left:10px
}
.posHint{
  position: absolute;
  top:75px;
  left:15px
}
#reRender{
  position: absolute;
  left:20px;
  top:25px;
  height:34px;
  width:85px;
  z-index: 1;
}
</style>
