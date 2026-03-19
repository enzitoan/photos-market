<template>
  <component :is="iconComponent" :size="size" :stroke-width="strokeWidth" :class="className" />
</template>

<script setup>
import { computed } from 'vue'
import {
  ShoppingCart,
  User,
  LogOut,
  Camera,
  PackageOpen,
  FileText,
  Settings,
  LayoutDashboard,
  Image,
  AlertTriangle,
  CheckCircle,
  XCircle,
  Clock,
  DollarSign,
  TrendingUp,
  Eye,
  EyeOff,
  RefreshCw,
  ChevronRight,
  ChevronDown,
  Info,
  Menu,
  X,
  Home,
  Mail,
  Phone,
  MapPin,
  Calendar,
  Download,
  Upload,
  Search,
  Filter,
  Plus,
  Minus,
  Check,
  Trash2,
  Edit,
  Save,
  Ban,
  Loader2,
  ArrowLeft,
  ArrowRight,
  ExternalLink,
  Copy,
  Share2,
  Heart,
  Star,
  Grid,
  List
} from 'lucide-vue-next'

const props = defineProps({
  name: {
    type: String,
    required: true
  },
  size: {
    type: [Number, String],
    default: 24
  },
  strokeWidth: {
    type: Number,
    default: 2
  },
  class: {
    type: String,
    default: ''
  }
})

const iconMap = {
  'shopping-cart': ShoppingCart,
  'user': User,
  'logout': LogOut,
  'camera': Camera,
  'package': PackageOpen,
  'file-text': FileText,
  'settings': Settings,
  'dashboard': LayoutDashboard,
  'image': Image,
  'alert': AlertTriangle,
  'check-circle': CheckCircle,
  'x-circle': XCircle,
  'clock': Clock,
  'dollar': DollarSign,
  'trending-up': TrendingUp,
  'eye': Eye,
  'eye-off': EyeOff,
  'refresh': RefreshCw,
  'chevron-right': ChevronRight,
  'chevron-down': ChevronDown,
  'info': Info,
  'menu': Menu,
  'x': X,
  'home': Home,
  'mail': Mail,
  'phone': Phone,
  'map-pin': MapPin,
  'calendar': Calendar,
  'download': Download,
  'upload': Upload,
  'search': Search,
  'filter': Filter,
  'plus': Plus,
  'minus': Minus,
  'check': Check,
  'trash': Trash2,
  'edit': Edit,
  'save': Save,
  'ban': Ban,
  'loader': Loader2,
  'arrow-left': ArrowLeft,
  'arrow-right': ArrowRight,
  'external-link': ExternalLink,
  'copy': Copy,
  'share': Share2,
  'heart': Heart,
  'star': Star,
  'grid': Grid,
  'list': List
}

const iconComponent = computed(() => iconMap[props.name] || Info)
const className = computed(() => props.class)
</script>
