<template>
  <q-layout view="lHh Lpr lFf">
    <q-header class="header sakolee-layout no-shadow">
      <q-toolbar class="header-top">
        <div class="sakolee-layout-header-left">
          <!-- Toggle -->
          <q-btn v-if="isLoggedIn" flat padding="0" class="text-black sakolee-layout-header-btn" aria-label="Menu" @click="toggleLeftDrawer">
            <span class="material-symbols-outlined fs-24">menu</span>
          </q-btn>
          <!-- Search Input -->
          <div class="global-search-input">
            <app-text-field placeholder="Global Search...">
              <template #prepend>
                <q-icon name="o_search" />
              </template>
            </app-text-field>
          </div>
          <q-btn flat no-caps class="no-padding q-ml-md" @click="$router.push('/')">
            <!-- Brand mark + wordmark, as the prototype sets them. The badge is the same SVG the tab
                 icon uses, so the two cannot drift; drawing it with a q-icon would need the filled
                 Material font, and only material-icons-outlined is loaded. -->
            <!-- <img :src="brandMark" alt="" width="34" height="34" class="brand-mark">
            <span class="text-weight-bold fs-18 text-primary q-ml-sm">Sakolee</span> -->
          </q-btn>
        </div>
        <!-- User menu when signed in, otherwise a login action -->
        <div class="sakolee-layout-header-right">
          <!-- Active Tenant switcher -->
          <q-btn-dropdown
            v-if="isLoggedIn && hasMultipleTenants"
            flat
            no-caps
            icon="o_apartment"
            :label="activeTenantLabel"
            class="no-padding tenant-scope-control"
          >
            <q-list>
              <q-item-label header class="text-grey-7">Switch tenant</q-item-label>
              <q-item
                v-for="t in assignments"
                :key="t.tenantId"
                v-close-popup
                clickable
                :active="t.tenantId === activeTenantId"
                @click="onSwitchTenant(t.tenantId)"
              >
                <q-item-section>
                  <q-item-label>{{ t.name || t.identifier }}</q-item-label>
                  <q-item-label caption class="text-capitalize">{{ (t.roleNames || []).join(", ") || "No roles" }}</q-item-label>
                </q-item-section>
                <q-item-section v-if="t.tenantId === activeTenantId" side>
                  <q-icon name="o_check" color="primary" />
                </q-item-section>
              </q-item>
            </q-list>
          </q-btn-dropdown>

          <!-- Active-tenant roles: the user's roles for the active tenant, shown on every
               authenticated screen alongside their name (in user-info). -->
          <!-- <div v-if="isLoggedIn && activeRoles.length" class="gt-xs row items-center q-gutter-xs">
            <q-chip
              v-for="r in activeRoles" :key="r" dense square color="teal-1" text-color="primary"
              class="text-capitalize q-my-none"
            >
              {{ r }}
            </q-chip>
          </div> -->

          <!-- Super-Admin tenant scope. -->
          <app-tenant-scope-select v-if="isLoggedIn" class="tenant-scope-select" />

          <!-- Help -->
          <div class="header-icon-btn" flat round dense>
            <span class="material-symbols-outlined fs-22 text-2e">help_outline</span>
          </div>
          <!-- Notification -->
          <notification-centre class="header-icon-btn" v-if="isLoggedIn" />
          <div class="line" />
          <!-- User Profile -->
          <user-info v-if="isLoggedIn" />
          <q-btn v-else unelevated color="primary" no-caps icon="o_login" label="Login" :to="{ name: 'login' }" />
        </div>
      </q-toolbar>
    </q-header>

    <!-- Collapsed, the drawer stays as a 60px icon rail. -->
    <!-- <q-drawer
      v-if="isLoggedIn"
      v-model="leftDrawerOpen"
      show-if-above
      :mini="menuCollapsed"
      :width="292"
      :mini-width="70"
      :breakpoint="1024"
      bordered
      class="bg-white"
    >
      <aside-header :class="menuCollapsed ? 'pa-3' : 'pa-6'" />
      <q-scroll-area class="fit">
        <AppMenu :mini="menuCollapsed" />
      </q-scroll-area>
    </q-drawer> -->

    <q-drawer
      v-if="isLoggedIn"
      v-model="leftDrawerOpen"
      show-if-above
      :mini="menuCollapsed"
      :width="279"
      :mini-width="90"
      :breakpoint="1023"
      bordered
      class="bg-white"
    >
      <div class="drawer-content">

        <!-- ========================================= -->
        <!-- 1. FIXED HEADER / LOGO                   -->
        <!-- ========================================= -->
        <div class="drawer-header">
          <aside-header />
        </div>

        <!-- ========================================= -->
        <!-- 2. ONLY THIS SECTION SCROLLS             -->
        <!-- ========================================= -->
        <q-scroll-area class="drawer-menu">
          <AppMenu :mini="menuCollapsed" />
        </q-scroll-area>

        <!-- ========================================= -->
        <!-- 3. FIXED BOTTOM MENU                     -->
        <!-- ========================================= -->
        <div class="drawer-footer">

          <!-- Settings -->
          <q-item
            v-ripple
            clickable
            class="drawer-footer-item"
            :to="{ name: 'settings' }"
          >
            <q-item-section avatar>
              <q-icon name="o_settings" size="24px" />
            </q-item-section>

            <q-item-section :class="menuCollapsed ? 'q-mini-drawer-hide' : ''">
              <q-item-label class="fw-500">Settings</q-item-label>
            </q-item-section>
          </q-item>

          <!-- Logout -->
          <q-item
            v-ripple
            clickable
            class="drawer-footer-item logout-item"
            @click="handleLogout"
          >
            <q-item-section avatar>
              <q-icon name="o_logout" size="24px" />
            </q-item-section>

            <q-item-section :class="menuCollapsed ? 'q-mini-drawer-hide' : ''">
              <q-item-label class="fw-500">Logout</q-item-label>
            </q-item-section>
          </q-item>

        </div>

      </div>
    </q-drawer>

    <q-page-container>
      <!-- The tenant scope is global and sticky (it survives reloads), so it is stated on every screen. -->
      <!-- inline-actions keeps the message and the button on ONE row; without it q-banner drops actions
           onto a second line and the banner takes twice the height on every page. -->
      <q-banner
        v-if="isLoggedIn && tenantScopeActive" dense inline-actions
        class="bg-orange-2 text-orange-10 q-px-md"
      >
        <template #avatar><q-icon name="o_visibility" color="orange-10" /></template>
        Viewing <span class="text-weight-bold">{{ scopedTenantName || "another tenant" }}</span> — changes apply to that tenant.
        <template #action>
          <q-btn flat dense no-caps color="orange-10" label="Back to my tenant" @click="clearScope" />
        </template>
      </q-banner>

      <router-view />
    </q-page-container>

    <!-- Universal Features floating sticky notes overlay (authenticated users only). -->
    <sticky-note-layer v-if="isLoggedIn" />

    <!-- <q-footer bordered class="bg-white">
      <div class="text-center q-py-sm">
        <h6 class="q-my-none text-black" style="font-size: 13px; font-weight: 400;">
          Copyright &copy; 2025 Vsky. Website Designed and Developed by
          <a href="https://www.vskysolutions.com/" target="_blank" style="text-decoration: none; color: #007bff;">
            VSky Solutions.
          </a>
        </h6>
      </div>
    </q-footer> -->
  </q-layout>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import { LocalStorage, Dialog, useQuasar } from "quasar";
import { storeToRefs } from "pinia";
import { useTenantStore } from "stores/tenant";
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";

import UserInfo from "shared/user_info.vue";
import AsideHeader from "shared/aside_header.vue";
import AppMenu from "src/components/app_menu.vue";
import NotificationCentre from "components/universal/NotificationCentre.vue";
import StickyNoteLayer from "components/universal/StickyNoteLayer.vue";
import AppTenantScopeSelect from "components/common/AppTenantScopeSelect.vue";
import { useTenantScope } from "composables/useTenantScope";

import AppTextField from "components/common/AppTextField.vue";

// Served straight out of public/ — the same file index.html points the tab icon at. Absolute,
// because history-mode routes would otherwise resolve it against the current path.
// const brandMark = "/icons/sakolee-mark.svg";

const authStore = useAuthStore();
const router = useRouter();
const $q = useQuasar();
const isLoggedIn = !!LocalStorage.getItem("token");

// Persist the drawer open/closed state across reloads (defaults to open). Above the breakpoint the
// drawer is always mounted (show-if-above) and this only governs the mobile overlay.
const DRAWER_KEY = "leftDrawerOpen";
const storedDrawer = LocalStorage.getItem(DRAWER_KEY);
const leftDrawerOpen = ref(storedDrawer === null ? true : storedDrawer);
watch(leftDrawerOpen, (value) => LocalStorage.set(DRAWER_KEY, value));

// …and remember whether the user collapsed it to the icon rail.
const MINI_KEY = "leftDrawerMini";
const menuCollapsed = ref(LocalStorage.getItem(MINI_KEY) === true);
watch(menuCollapsed, (value) => LocalStorage.set(MINI_KEY, value));

const toggleLeftDrawer = () => {
  // Below the breakpoint the drawer is an overlay and Quasar ignores mini entirely, so there the button
  // has to keep opening and closing it outright.
  if ($q.screen.lt.md) {
    leftDrawerOpen.value = !leftDrawerOpen.value;
    return;
  }
  menuCollapsed.value = !menuCollapsed.value;
};

// Super-Admin tenant scope: shown as a banner because the selection is global and survives a reload.
const {
  isScoped: tenantScopeActive, scopedTenantName, loadTenants: loadScopeTenants, clearScope
} = useTenantScope();
if (isLoggedIn) loadScopeTenants();

const tenantStore = useTenantStore();
const { assignments, activeTenantId } = storeToRefs(tenantStore);
const hasMultipleTenants = computed(() => tenantStore.hasMultipleTenants);
// The role names the user holds in the active tenant (multi-role), shown in the header.
const activeRoles = computed(() => tenantStore.activeRoles);
const activeTenantLabel = computed(() => {
  const t = tenantStore.activeTenant;
  return t?.name || t?.identifier || "Tenant";
});

// Confirm-before-discard when an open form has unsaved changes (AC-UI-007.4 guard).
const confirmDiscard = () => new Promise((resolve) => {
  Dialog.create({
    title: "Unsaved changes",
    message: "Switching tenant will discard your unsaved changes. Continue?",
    cancel: { label: "Cancel", flat: true, noCaps: true },
    ok: { label: "Continue", color: "primary", unelevated: true, noCaps: true },
    persistent: true
  }).onOk(() => resolve(true)).onCancel(() => resolve(false));
});

const onSwitchTenant = async (tenantId) => {
  const switched = await tenantStore.switchTenant(tenantId, { confirm: confirmDiscard });
  if (switched) {
    // Active page components listen and re-fetch their data for the new tenant.
    window.dispatchEvent(new CustomEvent("tenant-switched", { detail: { tenantId } }));
  }
};

const handleLogout = async () => {
  await authStore.logout();
  router.replace({ name: "login" });
};
</script>

<style scoped>
  .q-item.q-router-link--active, .q-item--active {
    color: #3ba5e5;
    font-weight: 500;
  }
  .no-underline {
    text-decoration: none !important;
  }

  /* The gradient and rounded corners live in the SVG itself; this only sets the drop shadow the
     prototype puts behind the badge. */
  .brand-mark {
    display: block;
    border-radius: 10px;
    box-shadow: 0 4px 12px rgba(70, 72, 212, 0.25);
  }

  /* =========================================================
   LEFT DRAWER STRUCTURE
   ========================================================= */

.drawer-content {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* Fixed logo/header area */
.drawer-header {
  flex: 0 0 auto;
}

/* Only this section is allowed to scroll */
.drawer-menu {
  flex: 1 1 auto;
  min-height: 0;
  padding: 0 14px;
}

/* Fixed bottom section */
.drawer-footer {
  flex: 0 0 auto;
  border-top: 1px solid #e5e7eb;
  background: #ffffff;
  padding: 8px 0;
}

/* Footer menu items */
.drawer-footer-item {
  min-height: 44px;
  color: #17233f;
  padding-left: 18px;
  padding-right: 18px;
}

/* Logout styling */
.logout-item {
  color: #dc2626;
}

.drawer-footer-item:hover {
  background: #f5f6ff;
}
</style>
