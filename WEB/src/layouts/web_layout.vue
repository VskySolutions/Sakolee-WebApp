<template>
  <q-layout view="lHh Lpr lFf" class="sakolee-marketing-layout">
    <q-header :class="['site-header', { 'is-scrolled': isScrolled }]" reveal="false" bordered="false">
      <div class="nav-wrap">
        <a href="#top" aria-label="Sakolee home" class="nav-brand-link" @click="closeMenu">
          <span class="brand">
            <span class="brand-mark" aria-hidden="true">
              <i /><i /><i />
            </span>
            <span>Sakolee</span>
          </span>
        </a>

        <button
          type="button"
          class="menu-button"
          :aria-expanded="mobileMenuOpen"
          aria-label="Toggle navigation"
          @click="mobileMenuOpen = !mobileMenuOpen"
        >
          <span :class="['menu-lines', { 'is-open': mobileMenuOpen }]" aria-hidden="true">
            <i /><i />
          </span>
        </button>

        <nav :class="['nav-links', { 'is-open': mobileMenuOpen }]" aria-label="Main navigation">
          <a href="#features" @click="closeMenu">Features</a>
          <a href="#preview" @click="closeMenu">Live preview</a>
          <a href="#pricing" @click="closeMenu">Pricing</a>
          <a href="#resources" @click="closeMenu">Resources</a>
        </nav>

        <a class="nav-cta" href="#demo" @click="closeMenu">
          Book a demo
          <svg viewBox="0 0 20 20" aria-hidden="true">
            <path d="M4 10h11M11 6l4 4-4 4" />
          </svg>
        </a>
      </div>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { onBeforeUnmount, onMounted, ref } from "vue";

const mobileMenuOpen = ref(false);
const isScrolled = ref(false);

const onScroll = () => {
  isScrolled.value = window.scrollY > 8;
};

const closeMenu = () => {
  mobileMenuOpen.value = false;
};

onMounted(() => {
  window.addEventListener("scroll", onScroll, { passive: true });
  onScroll();
});

onBeforeUnmount(() => {
  window.removeEventListener("scroll", onScroll);
});
</script>
