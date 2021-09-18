<template>
  <div id="app">
    <LeftWindow @reRend="rerender"></LeftWindow>
    <canvas-panel ref="canvasPanel"></canvas-panel>
  </div>
</template>

<script>
import LeftWindow from "./components/LeftWindow";
import CanvasPanel from "./components/CanvasPanel";

export default {
  name: 'App',
  components:{
    CanvasPanel,
    LeftWindow
  },
  data() {
    return {

    }
  },
  methods:{
    rerender(){
      this.$refs.canvasPanel.reRenderCanvas()
    }
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 10px;
}
</style>
