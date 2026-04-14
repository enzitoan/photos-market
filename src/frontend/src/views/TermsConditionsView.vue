<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />

    <main class="flex-1 bg-white py-12 sm:py-16">
      <div class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        <h1 class="text-3xl sm:text-4xl font-bold text-gray-900 mb-6">Términos y Condiciones</h1>

        <section class="space-y-8 text-gray-700 text-sm sm:text-base">
          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">1. Objeto</h2>
            <p>Estos términos regulan la compra de fotografías digitales ofrecidas en PhotosMarket. Al completar una compra, el cliente acepta estas condiciones y reconoce que está adquiriendo una licencia de uso de las imágenes, no la propiedad intelectual de las mismas.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">2. Productos</h2>
            <p>Las fotos se venden en formato digital y se entregan mediante un enlace de descarga. El comprador adquiere derechos de uso limitados tal como se describe en la plataforma.</p>
            <p>Queda prohibida la redistribución, reventa, sublicencia o publicación de las fotografías sin autorización expresa del fotógrafo.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">3. Derechos de uso</h2>
            <p>El cliente puede utilizar las imágenes para fines personales, promocionales o comerciales cuando así se indique. El fotógrafo conserva todos los derechos de autor y derechos de explotación.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">4. Pedido y pago</h2>
            <ul class="list-disc list-inside space-y-2">
              <li>El precio por foto se muestra en la plataforma y se calcula al crear el pedido.</li>
              <li>La compra se considera registrada cuando el pedido se crea en estado <strong>AwaitingPayment</strong>.</li>
              <li>Cuando el pago es aceptado, el pedido cambia al estado interno <strong>Processing</strong> y se registra la fecha de pago (<strong>PaidAt</strong>).</li>
              <li>El pedido se marca como completado y se genera el enlace de descarga cuando pasa a estado <strong>Completed</strong>.</li>
              <li>Los métodos de pago se gestionan fuera de la plataforma; aquí solo se registra la confirmación y la entrega digital.</li>
            </ul>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">5. Enlaces de descarga</h2>
            <p>Las descargas se entregan mediante enlaces únicos con fecha de expiración. El acceso a las fotos está condicionado al estado del pedido y a que el enlace siga siendo válido.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">6. Entrega y responsabilidad técnica</h2>
            <p>PhotosMarket proporciona el enlace de descarga, pero no se responsabiliza por fallos de red, correo electrónico o servicios de internet. El comprador debe descargar y guardar las imágenes dentro del periodo de validez del enlace.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">7. Devoluciones y reembolsos</h2>
            <p>Debido a la naturaleza digital del producto, los reembolsos no son automáticos. Se evaluarán caso a caso cuando exista un fallo legítimo en la entrega o una discrepancia clara respecto al producto descrito.</p>
            <p>No se aceptan devoluciones por cambios de opinión una vez que se ha proporcionado el enlace de descarga.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">8. Garantía</h2>
            <p>Las imágenes se entregan tal como aparecen en la plataforma. No se ofrecen garantías adicionales sobre la adaptación, edición o resultados de uso de las fotografías.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">9. Cancelación de pedidos</h2>
            <p>Si el pedido no se confirma como pagado en el plazo establecido, puede ser cancelado. El enlace de descarga deja de ser válido tras la cancelación.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">10. Protección de datos</h2>
            <p>PhotosMarket utiliza los datos de contacto y el correo electrónico únicamente para gestionar pedidos, envíos y notificaciones. No se comparte información personal con terceros fuera de lo necesario para procesar el pedido.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">11. Modificaciones</h2>
            <p>El fotógrafo puede actualizar estos términos cuando sea necesario. Las condiciones aplicables son las vigentes al momento de la compra.</p>
          </div>

          <div>
            <h2 class="text-xl sm:text-2xl font-semibold text-gray-900 mb-3">12. Ley aplicable</h2>
            <p>Estos términos se rigen por la legislación vigente del país del fotógrafo o de la operación comercial. Cualquier disputa se resolverá en los tribunales competentes establecidos.</p>
          </div>
        </section>

        <div class="mt-10 text-center">
          <router-link to="/" class="inline-flex items-center justify-center rounded-lg bg-primary-600 px-6 py-3 text-white font-semibold hover:bg-primary-700 transition-colors">
            Volver al inicio
          </router-link>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import NavBar from '@/components/NavBar.vue'
</script>
