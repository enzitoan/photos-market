<template>
  <div>
    <h1 class="text-2xl sm:text-3xl font-bold mb-6 sm:mb-8">Configuración del Sistema</h1>
    
    <!-- Watermark Configuration -->
    <div class="card mb-6">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-6 gap-3">
        <h2 class="text-lg sm:text-xl font-semibold">Marca de Agua</h2>
        <span 
          class="px-3 py-1 rounded-full text-xs font-medium inline-flex items-center w-fit"
          :class="hasUnsavedChanges ? 'bg-yellow-100 text-yellow-800' : 'bg-green-100 text-green-800'"
        >
          <Icon :name="hasUnsavedChanges ? 'alert' : 'check'" :size="14" class="mr-1" />
          {{ hasUnsavedChanges ? 'Cambios sin guardar' : 'Guardado' }}
        </span>
      </div>
      
      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Texto de la Marca de Agua
          </label>
          <input 
            type="text"
            v-model="watermarkText"
            @input="hasUnsavedChanges = true"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
            placeholder="© Mi Fotografía 2026"
            maxlength="50"
          >
          <p class="text-xs text-gray-500 mt-1">
            Este texto aparecerá como marca de agua en todas las fotos de vista previa
          </p>
        </div>
        
        <!-- Preview -->
        <div class="border-2 border-dashed border-gray-300 rounded-lg p-8">
          <h3 class="text-sm font-medium text-gray-700 mb-4">Vista Previa</h3>
          <div class="relative inline-block">
            <div class="w-80 h-60 bg-gradient-to-br from-gray-200 to-gray-300 rounded-lg flex items-center justify-center">
              <span class="text-gray-400 text-sm">Imagen de ejemplo</span>
            </div>
            <div 
              v-if="watermarkText"
              class="absolute inset-0 flex items-center justify-center pointer-events-none"
            >
              <div 
                class="text-white text-2xl font-bold opacity-30 rotate-[-30deg] select-none"
                style="text-shadow: 2px 2px 4px rgba(0,0,0,0.5)"
              >
                {{ watermarkText }}
              </div>
            </div>
          </div>
        </div>
        
        <!-- Save Button -->
<div class="flex flex-col sm:flex-row gap-3">
          <button 
            @click="saveWatermark"
            :disabled="!hasUnsavedChanges || saving"
            class="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
          >
            <Icon name="save" :size="18" class="mr-2" />
            {{ saving ? 'Guardando...' : 'Guardar Cambios' }}
          </button>
          
          <button 
            @click="resetWatermark"
            :disabled="!hasUnsavedChanges"
            class="btn btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Descartar Cambios
          </button>
        </div>
      </div>
    </div>
    
    <!-- Price Configuration -->
    <div class="card mb-6">
      <h2 class="text-lg sm:text-xl font-semibold mb-6">Precio por Foto</h2>
      
      <div class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Precio -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Precio por Foto
            </label>
            <div class="flex items-center gap-2">
              <span class="text-2xl font-bold">{{ currencySymbol }}</span>
              <input 
                type="number"
                v-model.number="photoPrice"
                @input="hasPriceChanges = true"
                min="0"
                step="1"
                class="w-32 px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 text-lg font-semibold"
              >
            </div>
            <p class="text-xs text-gray-500 mt-1">
              Este es el precio que se cobrará por cada foto individual
            </p>
          </div>
          
          <!-- Moneda -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Moneda
            </label>
            <select
              v-model="currency"
              @change="hasPriceChanges = true"
              class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 text-lg font-semibold"
            >
              <option value="CLP">🇨🇱 CLP - Peso Chileno</option>
              <option value="USD">🇺🇸 USD - Dólar</option>
              <option value="EUR">🇪🇺 EUR - Euro</option>
              <option value="ARS">🇦🇷 ARS - Peso Argentino</option>
              <option value="MXN">🇲🇽 MXN - Peso Mexicano</option>
              <option value="BRL">🇧🇷 BRL - Real Brasileño</option>
            </select>
            <p class="text-xs text-gray-500 mt-1">
              Selecciona la moneda en la que cobrarás
            </p>
          </div>
        </div>
        
        <div class="flex flex-col sm:flex-row gap-3">
          <button 
            @click="savePrice"
            :disabled="!hasPriceChanges || savingPrice"
            class="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
          >
            <Icon name="save" :size="18" class="mr-2" />
            {{ savingPrice ? 'Guardando...' : 'Guardar Precio' }}
          </button>
          
          <button 
            @click="resetPrice"
            :disabled="!hasPriceChanges"
            class="btn btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Descartar
          </button>
        </div>
      </div>
    </div>
    
    <!-- Discount Configuration -->
    <div class="card mb-6">
      <h2 class="text-lg sm:text-xl font-semibold mb-6">Sistema de Descuentos</h2>
      
      <div class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Mínimo de fotos -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Mínimo de Fotos para Descuento
            </label>
            <input 
              type="number"
              v-model.number="bulkDiscountMinPhotos"
              @input="hasDiscountChanges = true"
              min="2"
              step="1"
              class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 text-lg font-semibold"
            >
            <p class="text-xs text-gray-500 mt-1">
              Cantidad mínima de fotos para aplicar descuento
            </p>
          </div>
          
          <!-- Porcentaje de descuento -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Porcentaje de Descuento
            </label>
            <div class="flex items-center gap-2">
              <input 
                type="number"
                v-model.number="bulkDiscountPercentage"
                @input="hasDiscountChanges = true"
                min="0"
                max="100"
                step="1"
                class="w-32 px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 text-lg font-semibold"
              >
              <span class="text-2xl font-bold">%</span>
            </div>
            <p class="text-xs text-gray-500 mt-1">
              Porcentaje de descuento a aplicar
            </p>
          </div>
        </div>
        
        <!-- Preview de descuento -->
        <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <p class="text-sm text-blue-800 font-medium mb-2">Vista Previa del Descuento:</p>
          <p class="text-sm text-blue-700">
            Con <strong>{{ bulkDiscountMinPhotos }}</strong> fotos o más, los clientes obtendrán un <strong>{{ bulkDiscountPercentage }}%</strong> de descuento.
          </p>
          <p class="text-xs text-blue-600 mt-2">
            Ejemplo: {{ bulkDiscountMinPhotos }} fotos × {{ currencySymbol }}{{ photoPrice.toLocaleString('es-CL') }} = 
            <span class="line-through">{{ currencySymbol }}{{ (bulkDiscountMinPhotos * photoPrice).toLocaleString('es-CL') }}</span>
            → <strong>{{ currencySymbol }}{{ Math.round((bulkDiscountMinPhotos * photoPrice) * (1 - bulkDiscountPercentage / 100)).toLocaleString('es-CL') }}</strong>
          </p>
        </div>
        
        <div class="flex flex-col sm:flex-row gap-3">
          <button 
            @click="saveDiscount"
            :disabled="!hasDiscountChanges || savingDiscount"
            class="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
          >
            <Icon name="save" :size="18" class="mr-2" />
            {{ savingDiscount ? 'Guardando...' : 'Guardar Descuentos' }}
          </button>
          
          <button 
            @click="resetDiscount"
            :disabled="!hasDiscountChanges"
            class="btn btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Descartar
          </button>
        </div>
      </div>
    </div>
    
    <!-- System Info -->
    <div class="card bg-gray-50">
      <h2 class="text-lg sm:text-xl font-semibold mb-4">Información del Sistema</h2>
      
      <div class="space-y-2 text-sm">
        <div class="flex justify-between py-2 border-b border-gray-200">
          <span class="text-gray-600">Álbumes bloqueados:</span>
          <span class="font-medium">{{ adminStore.blockedAlbums.size }}</span>
        </div>
        
        <div class="flex justify-between py-2 border-b border-gray-200">
          <span class="text-gray-600">Marca de agua configurada:</span>
          <span class="font-medium">{{ adminStore.watermarkText ? 'Sí' : 'No' }}</span>
        </div>
        
        <div class="flex justify-between py-2">
          <span class="text-gray-600">Estado del sistema:</span>
          <span class="font-medium text-green-600 flex items-center">
            <Icon name="check" :size="16" class="mr-1" />
            Operativo
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import { useAdminStore } from '@/stores/admin'
import { useCartStore } from '@/stores/cart'
import Icon from '@/components/Icon.vue'
import adminService from '@/services/adminService'

const toast = useToast()
const adminStore = useAdminStore()
const cartStore = useCartStore()

const loadingSettings = ref(false)

const watermarkText = ref('')
const hasUnsavedChanges = ref(false)
const saving = ref(false)

const photoPrice = ref(1000.00)
const currency = ref('CLP')
const hasPriceChanges = ref(false)
const savingPrice = ref(false)

const bulkDiscountMinPhotos = ref(5)
const bulkDiscountPercentage = ref(20)
const hasDiscountChanges = ref(false)
const savingDiscount = ref(false)

const currencySymbol = computed(() => {
  const symbols = {
    'CLP': '$',
    'USD': '$',
    'EUR': '€',
    'ARS': '$',
    'MXN': '$',
    'BRL': 'R$'
  }
  return symbols[currency.value] || '$'
})

async function loadPhotographerSettings() {
  try {
    loadingSettings.value = true
    const response = await adminService.getPhotographerSettings()
    
    if (response.success && response.data) {
      watermarkText.value = response.data.watermarkText || ''
      photoPrice.value = response.data.photoPrice || 1000.00
      currency.value = response.data.currency || 'CLP'
      bulkDiscountMinPhotos.value = response.data.bulkDiscountMinPhotos || 5
      bulkDiscountPercentage.value = response.data.bulkDiscountPercentage || 20
      
      adminStore.setWatermarkText(response.data.watermarkText || '')
      
      // Persistir el precio y moneda en localStorage
      localStorage.setItem('photoPrice', photoPrice.value.toString())
      localStorage.setItem('currency', currency.value)
      
      console.log('Settings loaded:', {
        watermarkText: response.data.watermarkText,
        photoPrice: response.data.photoPrice,
        currency: response.data.currency,
        bulkDiscountMinPhotos: response.data.bulkDiscountMinPhotos,
        bulkDiscountPercentage: response.data.bulkDiscountPercentage
      })
    }
  } catch (error) {
    console.error('Error loading photographer settings:', error)
    // No mostrar error si simplemente no hay configuración
  } finally {
    loadingSettings.value = false
  }
}

async function saveWatermark() {
  saving.value = true
  
  try {
    const response = await adminService.updatePhotographerSettings({
      watermarkText: watermarkText.value,
      watermarkOpacity: 0.5,
      photoPrice: photoPrice.value,
      currency: currency.value,
      bulkDiscountMinPhotos: bulkDiscountMinPhotos.value,
      bulkDiscountPercentage: bulkDiscountPercentage.value
    })
    
    if (response.success) {
      adminStore.setWatermarkText(watermarkText.value)
      hasUnsavedChanges.value = false
      toast.success('Marca de agua guardada exitosamente')
    }
  } catch (error) {
    console.error('Error saving watermark:', error)
    toast.error('Error al guardar la marca de agua')
  } finally {
    saving.value = false
  }
}

function resetWatermark() {
  watermarkText.value = adminStore.watermarkText
  hasUnsavedChanges.value = false
  toast.info('Cambios descartados')
}

async function savePrice() {
  savingPrice.value = true
  
  try {
    const response = await adminService.updatePhotographerSettings({
      watermarkText: watermarkText.value,
      watermarkOpacity: 0.5,
      photoPrice: photoPrice.value,
      currency: currency.value,
      bulkDiscountMinPhotos: bulkDiscountMinPhotos.value,
      bulkDiscountPercentage: bulkDiscountPercentage.value
    })
    
    if (response.success) {
      hasPriceChanges.value = false
      
      // Persistir el precio y moneda en localStorage
      localStorage.setItem('photoPrice', photoPrice.value.toString())
      localStorage.setItem('currency', currency.value)
      
      // Recargar la configuración en los stores
      await adminStore.loadConfig()
      await cartStore.loadConfig()
      
      toast.success('Precio actualizado exitosamente')
      console.log('✅ Precio actualizado a:', photoPrice.value, currency.value)
      console.log('✅ Cart store price:', cartStore.pricePerPhoto)
    }
  } catch (error) {
    console.error('Error saving price:', error)
    toast.error('Error al guardar el precio')
  } finally {
    savingPrice.value = false
  }
}

async function resetPrice() {
  await loadPhotographerSettings()
  hasPriceChanges.value = false
  toast.info('Cambios descartados')
}

async function saveDiscount() {
  savingDiscount.value = true
  
  try {
    const response = await adminService.updatePhotographerSettings({
      watermarkText: watermarkText.value,
      watermarkOpacity: 0.5,
      photoPrice: photoPrice.value,
      currency: currency.value,
      bulkDiscountMinPhotos: bulkDiscountMinPhotos.value,
      bulkDiscountPercentage: bulkDiscountPercentage.value
    })
    
    if (response.success) {
      hasDiscountChanges.value = false
      
      // Recargar la configuración en los stores
      await adminStore.loadConfig()
      await cartStore.loadConfig()
      
      toast.success('Descuentos actualizados exitosamente')
      console.log('✅ Descuentos actualizados:', {
        minPhotos: bulkDiscountMinPhotos.value,
        percentage: bulkDiscountPercentage.value
      })
    }
  } catch (error) {
    console.error('Error saving discount:', error)
    toast.error('Error al guardar los descuentos')
  } finally {
    savingDiscount.value = false
  }
}

async function resetDiscount() {
  await loadPhotographerSettings()
  hasDiscountChanges.value = false
  toast.info('Cambios descartados')
}

onMounted(async () => {
  await loadPhotographerSettings()
})
</script>
