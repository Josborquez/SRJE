<template>
  <div
    class="file-upload"
    :class="{ 'drag-over': isDragOver }"
    @dragover.prevent="isDragOver = true"
    @dragleave="isDragOver = false"
    @drop.prevent="onDrop"
  >
    <div v-if="!selectedFile" class="upload-placeholder">
      <UploadCloud :size="40" class="upload-icon" />
      <p class="upload-title">Arrastra un archivo aqui</p>
      <p class="upload-subtitle">o seleccionalo desde tu computador</p>
      <label class="upload-btn">
        <FolderOpen :size="16" /> Seleccionar archivo
        <input type="file" :accept="accept" @change="onFileSelect" hidden />
      </label>
      <small v-if="accept" class="upload-formats">Formatos aceptados: {{ accept }}</small>
    </div>
    <div v-else class="file-info">
      <div class="file-info-detail">
        <FileCheck :size="20" class="file-info-icon" />
        <div>
          <span class="file-name">{{ selectedFile.name }}</span>
          <span class="file-size">{{ formatSize(selectedFile.size) }}</span>
        </div>
      </div>
      <button @click="clear" class="btn-clear">
        <Trash2 :size="14" /> Quitar
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { UploadCloud, FolderOpen, FileCheck, Trash2 } from 'lucide-vue-next'

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
  border: 2px dashed #cbd5e1;
  border-radius: 12px;
  padding: 2.5rem 2rem;
  text-align: center;
  transition: all 0.2s;
  background: #fafbfc;
}
.file-upload:hover { border-color: #94a3b8; }
.file-upload.drag-over { border-color: #2563eb; background: #eff6ff; }

.upload-placeholder { display: flex; flex-direction: column; align-items: center; gap: 0.3rem; }
.upload-icon { color: #94a3b8; margin-bottom: 0.5rem; }
.upload-title { font-size: 1rem; font-weight: 500; color: #334155; }
.upload-subtitle { font-size: 0.85rem; color: #94a3b8; margin-bottom: 0.5rem; }

.upload-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.55rem 1.1rem;
  background: #2563eb;
  color: #fff;
  border-radius: 8px;
  cursor: pointer;
  margin: 0.5rem 0;
  font-size: 0.88rem;
  font-weight: 500;
  transition: background 0.2s;
}
.upload-btn:hover { background: #1d4ed8; }
.upload-formats { color: #94a3b8; font-size: 0.78rem; margin-top: 0.3rem; }

.file-info {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
}
.file-info-detail {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}
.file-info-icon { color: #16a34a; }
.file-info-detail div { display: flex; flex-direction: column; text-align: left; }
.file-name { font-weight: 500; font-size: 0.92rem; color: #1e293b; }
.file-size { font-size: 0.8rem; color: #94a3b8; }
.btn-clear {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  background: none;
  border: 1px solid #e2e8f0;
  padding: 0.35rem 0.7rem;
  border-radius: 6px;
  cursor: pointer;
  color: #64748b;
  font-size: 0.82rem;
  transition: all 0.2s;
}
.btn-clear:hover { border-color: #dc2626; color: #dc2626; background: #fef2f2; }
</style>
