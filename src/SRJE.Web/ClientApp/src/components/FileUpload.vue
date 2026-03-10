<template>
  <div
    class="file-upload"
    :class="{ 'drag-over': isDragOver }"
    @dragover.prevent="isDragOver = true"
    @dragleave="isDragOver = false"
    @drop.prevent="onDrop"
  >
    <div v-if="!selectedFile" class="upload-placeholder">
      <p>Arrastra un archivo aqui o</p>
      <label class="upload-btn">
        Seleccionar archivo
        <input type="file" :accept="accept" @change="onFileSelect" hidden />
      </label>
      <small v-if="accept">Formatos aceptados: {{ accept }}</small>
    </div>
    <div v-else class="file-info">
      <span>{{ selectedFile.name }} ({{ formatSize(selectedFile.size) }})</span>
      <button @click="clear" class="btn-clear">Quitar</button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  accept: { type: String, default: '' }
})

const emit = defineEmits(['fileSelected'])

const selectedFile = ref(null)
const isDragOver = ref(false)

function onFileSelect(e) {
  const file = e.target.files[0]
  if (file) setFile(file)
}

function onDrop(e) {
  isDragOver.value = false
  const file = e.dataTransfer.files[0]
  if (file) setFile(file)
}

function setFile(file) {
  selectedFile.value = file
  emit('fileSelected', file)
}

function clear() {
  selectedFile.value = null
  emit('fileSelected', null)
}

function formatSize(bytes) {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / 1048576).toFixed(1) + ' MB'
}
</script>

<style scoped>
.file-upload {
  border: 2px dashed #ccc; border-radius: 8px; padding: 2rem;
  text-align: center; transition: border-color 0.2s; background: #fafafa;
}
.file-upload.drag-over { border-color: #4fc3f7; background: #e3f2fd; }
.upload-btn {
  display: inline-block; padding: 0.5rem 1rem; background: #1976d2; color: #fff;
  border-radius: 4px; cursor: pointer; margin: 0.5rem 0;
}
.upload-btn:hover { background: #1565c0; }
.file-info { display: flex; align-items: center; justify-content: center; gap: 1rem; }
.btn-clear { background: none; border: 1px solid #999; padding: 0.3rem 0.6rem; border-radius: 4px; cursor: pointer; }
</style>
