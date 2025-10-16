# 🎨 Guía de Implementación Frontend - CrudPark Vue.js

## 📋 **Resumen del Proyecto**

Este documento describe cómo implementar el frontend del sistema de gestión de parqueadero usando **Vue.js 3** con **Composition API**, conectándose al backend API desarrollado en C# con ASP.NET Core.

## 🛠️ **Stack Tecnológico Recomendado**

### **Core Framework**
- **Vue.js 3** - Framework principal con Composition API
- **TypeScript** - Tipado estático para mejor desarrollo
- **Vite** - Build tool rápido y moderno

### **UI Framework**
- **Vuetify 3** - Componentes Material Design
- **Vue Router 4** - Enrutamiento SPA
- **Pinia** - Gestión de estado

### **HTTP Client**
- **Axios** - Cliente HTTP para comunicación con API
- **Vue Query (TanStack Query)** - Cache y sincronización de datos

### **Utilidades**
- **Chart.js** - Gráficas para dashboard
- **Day.js** - Manipulación de fechas
- **VueUse** - Utilidades composables

## 🏗️ **Estructura del Proyecto**

```
crudpark-frontend/
├── src/
│   ├── components/           # Componentes reutilizables
│   │   ├── common/          # Componentes comunes
│   │   ├── forms/           # Formularios
│   │   └── charts/          # Gráficas
│   ├── views/               # Páginas/Vistas
│   │   ├── Dashboard.vue
│   │   ├── Operadores.vue
│   │   ├── Mensualidades.vue
│   │   ├── Tarifas.vue
│   │   ├── Ingresos.vue
│   │   ├── Turnos.vue
│   │   └── Reportes.vue
│   ├── stores/              # Pinia stores
│   │   ├── operadores.ts
│   │   ├── mensualidades.ts
│   │   ├── tarifas.ts
│   │   ├── ingresos.ts
│   │   ├── turnos.ts
│   │   └── dashboard.ts
│   ├── services/            # Servicios API
│   │   ├── api.ts
│   │   ├── operadores.ts
│   │   ├── mensualidades.ts
│   │   ├── tarifas.ts
│   │   ├── ingresos.ts
│   │   ├── turnos.ts
│   │   └── reportes.ts
│   ├── types/               # Tipos TypeScript
│   │   ├── operador.ts
│   │   ├── mensualidad.ts
│   │   ├── tarifa.ts
│   │   ├── ingreso.ts
│   │   ├── turno.ts
│   │   └── dashboard.ts
│   ├── composables/         # Composables personalizados
│   │   ├── useApi.ts
│   │   ├── useNotifications.ts
│   │   └── useValidation.ts
│   ├── router/              # Configuración de rutas
│   │   └── index.ts
│   ├── assets/              # Recursos estáticos
│   └── App.vue
├── public/
├── package.json
├── vite.config.ts
├── tsconfig.json
└── README.md
```

## 🚀 **Configuración Inicial**

### **1. Crear Proyecto Vue.js**

```bash
# Crear proyecto con Vite
npm create vue@latest crudpark-frontend

# Seleccionar opciones:
# ✅ TypeScript
# ✅ Router
# ✅ Pinia
# ✅ ESLint
# ✅ Prettier

cd crudpark-frontend
npm install
```

### **2. Instalar Dependencias Adicionales**

```bash
# UI Framework
npm install vuetify@next @mdi/font

# HTTP Client y Cache
npm install axios @tanstack/vue-query

# Gráficas
npm install chart.js vue-chartjs

# Utilidades
npm install dayjs @vueuse/core

# Desarrollo
npm install -D @types/node
```

### **3. Configurar Vite**

```typescript
// vite.config.ts
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true
      }
    }
  }
})
```

## 🔧 **Configuración de Servicios**

### **1. Cliente HTTP Base**

```typescript
// src/services/api.ts
import axios from 'axios'
import { useNotificationStore } from '@/stores/notifications'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Interceptor de respuestas
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const notificationStore = useNotificationStore()
    
    if (error.response?.status === 500) {
      notificationStore.showError('Error interno del servidor')
    } else if (error.response?.data?.message) {
      notificationStore.showError(error.response.data.message)
    } else {
      notificationStore.showError('Error de conexión')
    }
    
    return Promise.reject(error)
  }
)

export default api
```

### **2. Servicio de Operadores**

```typescript
// src/services/operadores.ts
import api from './api'
import type { Operador, CreateOperadorDTO, UpdateOperadorDTO } from '@/types/operador'

export const operadoresService = {
  // Obtener todos los operadores
  async getAll(): Promise<Operador[]> {
    const response = await api.get('/operadores')
    return response.data
  },

  // Obtener operador por ID
  async getById(id: number): Promise<Operador> {
    const response = await api.get(`/operadores/${id}`)
    return response.data
  },

  // Crear operador
  async create(data: CreateOperadorDTO): Promise<Operador> {
    const response = await api.post('/operadores', data)
    return response.data
  },

  // Actualizar operador
  async update(id: number, data: UpdateOperadorDTO): Promise<Operador> {
    const response = await api.put(`/operadores/${id}`, data)
    return response.data
  },

  // Eliminar operador
  async delete(id: number): Promise<void> {
    await api.delete(`/operadores/${id}`)
  }
}
```

### **3. Tipos TypeScript**

```typescript
// src/types/operador.ts
export interface Operador {
  id: number
  nombre: string
  email?: string
  isActive: boolean
  createdAt: string
}

export interface CreateOperadorDTO {
  nombre: string
  email?: string
}

export interface UpdateOperadorDTO {
  nombre: string
  email?: string
  isActive: boolean
}
```

## 🏪 **Gestión de Estado con Pinia**

### **1. Store de Operadores**

```typescript
// src/stores/operadores.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { operadoresService } from '@/services/operadores'
import type { Operador, CreateOperadorDTO, UpdateOperadorDTO } from '@/types/operador'

export const useOperadoresStore = defineStore('operadores', () => {
  // Estado
  const operadores = ref<Operador[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const operadoresActivos = computed(() => 
    operadores.value.filter(op => op.isActive)
  )

  // Actions
  async function fetchOperadores() {
    loading.value = true
    error.value = null
    try {
      operadores.value = await operadoresService.getAll()
    } catch (err) {
      error.value = 'Error al cargar operadores'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function createOperador(data: CreateOperadorDTO) {
    loading.value = true
    try {
      const nuevoOperador = await operadoresService.create(data)
      operadores.value.push(nuevoOperador)
      return nuevoOperador
    } catch (err) {
      error.value = 'Error al crear operador'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function updateOperador(id: number, data: UpdateOperadorDTO) {
    loading.value = true
    try {
      const operadorActualizado = await operadoresService.update(id, data)
      const index = operadores.value.findIndex(op => op.id === id)
      if (index !== -1) {
        operadores.value[index] = operadorActualizado
      }
      return operadorActualizado
    } catch (err) {
      error.value = 'Error al actualizar operador'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function deleteOperador(id: number) {
    loading.value = true
    try {
      await operadoresService.delete(id)
      operadores.value = operadores.value.filter(op => op.id !== id)
    } catch (err) {
      error.value = 'Error al eliminar operador'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // Estado
    operadores,
    loading,
    error,
    // Getters
    operadoresActivos,
    // Actions
    fetchOperadores,
    createOperador,
    updateOperador,
    deleteOperador
  }
})
```

## 🎨 **Componentes Vue.js**

### **1. Vista de Operadores**

```vue
<!-- src/views/Operadores.vue -->
<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title>
            <span class="text-h5">Gestión de Operadores</span>
            <v-spacer />
            <v-btn
              color="primary"
              @click="openCreateDialog"
              :loading="loading"
            >
              <v-icon left>mdi-plus</v-icon>
              Nuevo Operador
            </v-btn>
          </v-card-title>

          <v-card-text>
            <v-data-table
              :headers="headers"
              :items="operadores"
              :loading="loading"
              class="elevation-1"
            >
              <template v-slot:item.isActive="{ item }">
                <v-chip
                  :color="item.isActive ? 'success' : 'error'"
                  small
                >
                  {{ item.isActive ? 'Activo' : 'Inactivo' }}
                </v-chip>
              </template>

              <template v-slot:item.actions="{ item }">
                <v-btn
                  icon
                  small
                  @click="openEditDialog(item)"
                >
                  <v-icon>mdi-pencil</v-icon>
                </v-btn>
                <v-btn
                  icon
                  small
                  color="error"
                  @click="confirmDelete(item)"
                >
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Dialog para crear/editar -->
    <OperadorDialog
      v-model="dialog"
      :operador="selectedOperador"
      :is-edit="isEdit"
      @save="handleSave"
    />
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useOperadoresStore } from '@/stores/operadores'
import type { Operador, CreateOperadorDTO, UpdateOperadorDTO } from '@/types/operador'
import OperadorDialog from '@/components/forms/OperadorDialog.vue'

const operadoresStore = useOperadoresStore()
const { operadores, loading } = storeToRefs(operadoresStore)

// Estado local
const dialog = ref(false)
const selectedOperador = ref<Operador | null>(null)
const isEdit = ref(false)

// Headers de la tabla
const headers = [
  { title: 'ID', key: 'id', sortable: true },
  { title: 'Nombre', key: 'nombre', sortable: true },
  { title: 'Email', key: 'email', sortable: false },
  { title: 'Estado', key: 'isActive', sortable: true },
  { title: 'Fecha Creación', key: 'createdAt', sortable: true },
  { title: 'Acciones', key: 'actions', sortable: false }
]

// Métodos
function openCreateDialog() {
  selectedOperador.value = null
  isEdit.value = false
  dialog.value = true
}

function openEditDialog(operador: Operador) {
  selectedOperador.value = { ...operador }
  isEdit.value = true
  dialog.value = true
}

async function handleSave(data: CreateOperadorDTO | UpdateOperadorDTO) {
  try {
    if (isEdit.value && selectedOperador.value) {
      await operadoresStore.updateOperador(selectedOperador.value.id, data as UpdateOperadorDTO)
    } else {
      await operadoresStore.createOperador(data as CreateOperadorDTO)
    }
    dialog.value = false
  } catch (error) {
    // Error manejado por el store
  }
}

function confirmDelete(operador: Operador) {
  // Implementar confirmación de eliminación
}

// Lifecycle
onMounted(() => {
  operadoresStore.fetchOperadores()
})
</script>
```

### **2. Formulario de Operador**

```vue
<!-- src/components/forms/OperadorDialog.vue -->
<template>
  <v-dialog
    v-model="localDialog"
    max-width="500px"
    persistent
  >
    <v-card>
      <v-card-title>
        {{ isEdit ? 'Editar Operador' : 'Nuevo Operador' }}
      </v-card-title>

      <v-card-text>
        <v-form
          ref="form"
          v-model="valid"
          @submit.prevent="handleSubmit"
        >
          <v-text-field
            v-model="formData.nombre"
            label="Nombre"
            :rules="nombreRules"
            required
            outlined
          />

          <v-text-field
            v-model="formData.email"
            label="Email"
            type="email"
            :rules="emailRules"
            outlined
          />
        </v-form>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn
          color="grey"
          text
          @click="closeDialog"
        >
          Cancelar
        </v-btn>
        <v-btn
          color="primary"
          :disabled="!valid"
          :loading="loading"
          @click="handleSubmit"
        >
          {{ isEdit ? 'Actualizar' : 'Crear' }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { Operador, CreateOperadorDTO, UpdateOperadorDTO } from '@/types/operador'

// Props
interface Props {
  modelValue: boolean
  operador?: Operador | null
  isEdit?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  operador: null,
  isEdit: false
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'save': [data: CreateOperadorDTO | UpdateOperadorDTO]
}>()

// Estado local
const form = ref()
const valid = ref(false)
const loading = ref(false)

const formData = ref({
  nombre: '',
  email: ''
})

// Computed
const localDialog = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

// Validaciones
const nombreRules = [
  (v: string) => !!v || 'El nombre es requerido',
  (v: string) => v.length <= 100 || 'El nombre no puede exceder 100 caracteres'
]

const emailRules = [
  (v: string) => !v || /.+@.+\..+/.test(v) || 'El email debe ser válido',
  (v: string) => !v || v.length <= 200 || 'El email no puede exceder 200 caracteres'
]

// Métodos
function closeDialog() {
  localDialog.value = false
  resetForm()
}

function resetForm() {
  formData.value = {
    nombre: '',
    email: ''
  }
  form.value?.resetValidation()
}

async function handleSubmit() {
  const { valid: isValid } = await form.value.validate()
  if (!isValid) return

  loading.value = true
  try {
    const data = { ...formData.value }
    emit('save', data)
  } finally {
    loading.value = false
  }
}

// Watchers
watch(() => props.operador, (newOperador) => {
  if (newOperador) {
    formData.value = {
      nombre: newOperador.nombre,
      email: newOperador.email || ''
    }
  } else {
    resetForm()
  }
}, { immediate: true })
</script>
```

## 📊 **Dashboard con Gráficas**

### **1. Vista de Dashboard**

```vue
<!-- src/views/Dashboard.vue -->
<template>
  <v-container>
    <v-row>
      <!-- Métricas principales -->
      <v-col cols="12" md="3">
        <v-card class="text-center">
          <v-card-text>
            <v-icon size="48" color="primary">mdi-car</v-icon>
            <div class="text-h4 mt-2">{{ dashboardData.vehiculosActualmenteEnParqueadero }}</div>
            <div class="text-subtitle-1">Vehículos en Parqueadero</div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="3">
        <v-card class="text-center">
          <v-card-text>
            <v-icon size="48" color="success">mdi-currency-usd</v-icon>
            <div class="text-h4 mt-2">${{ formatCurrency(dashboardData.ingresosDelDia) }}</div>
            <div class="text-subtitle-1">Ingresos del Día</div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="3">
        <v-card class="text-center">
          <v-card-text>
            <v-icon size="48" color="info">mdi-calendar-check</v-icon>
            <div class="text-h4 mt-2">{{ dashboardData.mensualidadesActivas }}</div>
            <div class="text-subtitle-1">Mensualidades Activas</div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="3">
        <v-card class="text-center">
          <v-card-text>
            <v-icon size="48" color="warning">mdi-alert</v-icon>
            <div class="text-h4 mt-2">{{ dashboardData.mensualidadesProximasVencer }}</div>
            <div class="text-subtitle-1">Próximas a Vencer</div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <!-- Gráfica de ingresos por hora -->
      <v-col cols="12" md="8">
        <v-card>
          <v-card-title>Ingresos por Hora</v-card-title>
          <v-card-text>
            <IngresosPorHoraChart :data="dashboardData.ingresosPorHora" />
          </v-card-text>
        </v-card>
      </v-col>

      <!-- Mensualidades próximas a vencer -->
      <v-col cols="12" md="4">
        <v-card>
          <v-card-title>Mensualidades Próximas a Vencer</v-card-title>
          <v-card-text>
            <v-list>
              <v-list-item
                v-for="mensualidad in dashboardData.mensualidadesProximas"
                :key="mensualidad.id"
              >
                <v-list-item-content>
                  <v-list-item-title>{{ mensualidad.nombre }}</v-list-item-title>
                  <v-list-item-subtitle>
                    {{ mensualidad.placa }} - {{ mensualidad.diasRestantes }} días
                  </v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useDashboardStore } from '@/stores/dashboard'
import IngresosPorHoraChart from '@/components/charts/IngresosPorHoraChart.vue'
import { formatCurrency } from '@/utils/formatters'

const dashboardStore = useDashboardStore()
const { dashboardData, loading } = storeToRefs(dashboardStore)

onMounted(() => {
  dashboardStore.fetchDashboardData()
})
</script>
```

### **2. Componente de Gráfica**

```vue
<!-- src/components/charts/IngresosPorHoraChart.vue -->
<template>
  <div class="chart-container">
    <canvas ref="chartCanvas"></canvas>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'
import type { IngresoDiarioDTO } from '@/types/dashboard'

// Registrar componentes de Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
)

// Props
interface Props {
  data: IngresoDiarioDTO[]
}

const props = defineProps<Props>()

// Estado local
const chartCanvas = ref<HTMLCanvasElement>()
let chart: ChartJS | null = null

// Métodos
function createChart() {
  if (!chartCanvas.value) return

  const ctx = chartCanvas.value.getContext('2d')
  if (!ctx) return

  chart = new ChartJS(ctx, {
    type: 'bar',
    data: {
      labels: props.data.map(item => `${item.hora}:00`),
      datasets: [
        {
          label: 'Cantidad de Ingresos',
          data: props.data.map(item => item.cantidadIngresos),
          backgroundColor: 'rgba(54, 162, 235, 0.5)',
          borderColor: 'rgba(54, 162, 235, 1)',
          borderWidth: 1
        },
        {
          label: 'Valor Total',
          data: props.data.map(item => item.valorTotal),
          backgroundColor: 'rgba(75, 192, 192, 0.5)',
          borderColor: 'rgba(75, 192, 192, 1)',
          borderWidth: 1,
          yAxisID: 'y1'
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: {
          type: 'linear',
          display: true,
          position: 'left',
          title: {
            display: true,
            text: 'Cantidad de Ingresos'
          }
        },
        y1: {
          type: 'linear',
          display: true,
          position: 'right',
          title: {
            display: true,
            text: 'Valor Total ($)'
          },
          grid: {
            drawOnChartArea: false
          }
        }
      },
      plugins: {
        title: {
          display: true,
          text: 'Ingresos por Hora del Día'
        }
      }
    }
  })
}

function updateChart() {
  if (!chart) return

  chart.data.labels = props.data.map(item => `${item.hora}:00`)
  chart.data.datasets[0].data = props.data.map(item => item.cantidadIngresos)
  chart.data.datasets[1].data = props.data.map(item => item.valorTotal)
  chart.update()
}

// Lifecycle
onMounted(() => {
  createChart()
})

// Watchers
watch(() => props.data, () => {
  updateChart()
}, { deep: true })
</script>

<style scoped>
.chart-container {
  position: relative;
  height: 400px;
  width: 100%;
}
</style>
```

## 🛣️ **Configuración de Rutas**

```typescript
// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '@/views/Dashboard.vue'
import Operadores from '@/views/Operadores.vue'
import Mensualidades from '@/views/Mensualidades.vue'
import Tarifas from '@/views/Tarifas.vue'
import Ingresos from '@/views/Ingresos.vue'
import Turnos from '@/views/Turnos.vue'
import Reportes from '@/views/Reportes.vue'

const routes = [
  {
    path: '/',
    name: 'Dashboard',
    component: Dashboard
  },
  {
    path: '/operadores',
    name: 'Operadores',
    component: Operadores
  },
  {
    path: '/mensualidades',
    name: 'Mensualidades',
    component: Mensualidades
  },
  {
    path: '/tarifas',
    name: 'Tarifas',
    component: Tarifas
  },
  {
    path: '/ingresos',
    name: 'Ingresos',
    component: Ingresos
  },
  {
    path: '/turnos',
    name: 'Turnos',
    component: Turnos
  },
  {
    path: '/reportes',
    name: 'Reportes',
    component: Reportes
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
```

## 🎨 **Configuración de Vuetify**

```typescript
// src/plugins/vuetify.ts
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { mdi } from 'vuetify/iconsets/mdi'
import '@mdi/font/css/materialdesignicons.css'

export default createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi',
    sets: {
      mdi
    }
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: '#1976D2',
          secondary: '#424242',
          accent: '#82B1FF',
          error: '#FF5252',
          info: '#2196F3',
          success: '#4CAF50',
          warning: '#FFC107'
        }
      }
    }
  }
})
```

## 📱 **Layout Principal**

```vue
<!-- src/App.vue -->
<template>
  <v-app>
    <v-navigation-drawer
      v-model="drawer"
      app
    >
      <v-list>
        <v-list-item
          v-for="item in menuItems"
          :key="item.title"
          :to="item.to"
          link
        >
          <template v-slot:prepend>
            <v-icon>{{ item.icon }}</v-icon>
          </template>
          <v-list-item-title>{{ item.title }}</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>

    <v-app-bar app>
      <v-app-bar-nav-icon @click="drawer = !drawer" />
      <v-toolbar-title>CrudPark - Sistema de Parqueadero</v-toolbar-title>
    </v-app-bar>

    <v-main>
      <router-view />
    </v-main>

    <!-- Notificaciones -->
    <v-snackbar
      v-model="notificationStore.show"
      :color="notificationStore.color"
      :timeout="notificationStore.timeout"
    >
      {{ notificationStore.message }}
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useNotificationStore } from '@/stores/notifications'

const notificationStore = useNotificationStore()

const drawer = ref(true)

const menuItems = [
  { title: 'Dashboard', icon: 'mdi-view-dashboard', to: '/' },
  { title: 'Operadores', icon: 'mdi-account-group', to: '/operadores' },
  { title: 'Mensualidades', icon: 'mdi-calendar-check', to: '/mensualidades' },
  { title: 'Tarifas', icon: 'mdi-currency-usd', to: '/tarifas' },
  { title: 'Ingresos', icon: 'mdi-car', to: '/ingresos' },
  { title: 'Turnos', icon: 'mdi-clock', to: '/turnos' },
  { title: 'Reportes', icon: 'mdi-chart-line', to: '/reportes' }
]
</script>
```

## 🔧 **Utilidades y Composables**

### **1. Composable para API**

```typescript
// src/composables/useApi.ts
import { ref } from 'vue'
import api from '@/services/api'

export function useApi() {
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function execute<T>(apiCall: () => Promise<T>): Promise<T | null> {
    loading.value = true
    error.value = null
    
    try {
      const result = await apiCall()
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Error desconocido'
      return null
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    execute
  }
}
```

### **2. Composable para Notificaciones**

```typescript
// src/composables/useNotifications.ts
import { useNotificationStore } from '@/stores/notifications'

export function useNotifications() {
  const notificationStore = useNotificationStore()

  function showSuccess(message: string) {
    notificationStore.showSuccess(message)
  }

  function showError(message: string) {
    notificationStore.showError(message)
  }

  function showInfo(message: string) {
    notificationStore.showInfo(message)
  }

  function showWarning(message: string) {
    notificationStore.showWarning(message)
  }

  return {
    showSuccess,
    showError,
    showInfo,
    showWarning
  }
}
```

## 📦 **Scripts de Package.json**

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vue-tsc && vite build",
    "preview": "vite preview",
    "lint": "eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --fix --ignore-path .gitignore",
    "type-check": "vue-tsc --noEmit"
  }
}
```

## 🚀 **Comandos de Desarrollo**

```bash
# Desarrollo
npm run dev

# Build para producción
npm run build

# Preview de build
npm run preview

# Linting
npm run lint

# Type checking
npm run type-check
```

## 🔗 **Integración con Backend**

### **Variables de Entorno**

```bash
# .env.development
VITE_API_URL=http://localhost:8080/api

# .env.production
VITE_API_URL=https://tu-dominio.com/api
```

### **Configuración de CORS**

El backend ya tiene CORS configurado para permitir todas las conexiones. Si necesitas restringir:

```csharp
// En Program.cs del backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://tu-dominio.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## 📋 **Checklist de Implementación**

### **Fase 1: Configuración Base**
- [ ] Crear proyecto Vue.js con Vite
- [ ] Instalar y configurar Vuetify
- [ ] Configurar TypeScript
- [ ] Configurar Vue Router
- [ ] Configurar Pinia
- [ ] Configurar Axios

### **Fase 2: Servicios y Tipos**
- [ ] Crear tipos TypeScript para todas las entidades
- [ ] Implementar servicios API
- [ ] Crear stores de Pinia
- [ ] Configurar interceptores de Axios

### **Fase 3: Componentes Base**
- [ ] Crear layout principal
- [ ] Implementar navegación
- [ ] Crear componentes de formularios
- [ ] Implementar sistema de notificaciones

### **Fase 4: Vistas Principales**
- [ ] Dashboard con métricas y gráficas
- [ ] Gestión de Operadores
- [ ] Gestión de Mensualidades
- [ ] Gestión de Tarifas
- [ ] Control de Ingresos
- [ ] Sistema de Turnos
- [ ] Reportes y exportación

### **Fase 5: Funcionalidades Avanzadas**
- [ ] Gráficas interactivas
- [ ] Filtros y búsquedas
- [ ] Paginación
- [ ] Exportación de datos
- [ ] Notificaciones en tiempo real
- [ ] Responsive design

### **Fase 6: Testing y Optimización**
- [ ] Tests unitarios
- [ ] Tests de integración
- [ ] Optimización de rendimiento
- [ ] SEO y meta tags
- [ ] PWA (opcional)

## 🎯 **Características Destacadas**

### **1. Dashboard en Tiempo Real**
- Métricas principales con iconos
- Gráficas de ingresos por hora
- Lista de mensualidades próximas a vencer
- Actualización automática de datos

### **2. Gestión Completa**
- CRUD para todas las entidades
- Validaciones en tiempo real
- Confirmaciones de eliminación
- Estados de carga

### **3. Experiencia de Usuario**
- Diseño Material Design
- Responsive para móviles
- Notificaciones toast
- Navegación intuitiva

### **4. Funcionalidades Avanzadas**
- Gráficas interactivas con Chart.js
- Exportación de reportes
- Filtros y búsquedas
- Paginación de tablas

## 🚀 **Despliegue**

### **Build para Producción**
```bash
npm run build
```

### **Despliegue en Netlify/Vercel**
```bash
# Netlify
npm run build
# Subir carpeta dist/

# Vercel
vercel --prod
```

### **Docker (Opcional)**
```dockerfile
# Dockerfile
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## 📚 **Recursos Adicionales**

- [Vue.js 3 Documentation](https://vuejs.org/)
- [Vuetify 3 Documentation](https://vuetifyjs.com/)
- [Pinia Documentation](https://pinia.vuejs.org/)
- [Vue Router Documentation](https://router.vuejs.org/)
- [Chart.js Documentation](https://www.chartjs.org/)
- [TypeScript Documentation](https://www.typescriptlang.org/)

---

**¡Con esta guía tienes todo lo necesario para implementar un frontend completo y moderno para el sistema de parqueadero!** 🎉

El frontend se conectará perfectamente con el backend API que desarrollamos, proporcionando una experiencia de usuario completa y profesional.
