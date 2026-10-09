<template>
  <div class="aside-header">
    <!-- Product -->
    <div class="brand-section cursor-pointer" :class="menuCollapsed ? 'pa-10' : 'pa-20'" @click="$router.push('/dashboard')">
      <div class="brand-logo">
        <span class="material-symbols-outlined fs-26" style="font-variation-settings: 'FILL' 1;">school</span>
      </div>
      <div v-if="!menuCollapsed" class="brand-name q-mini-drawer-hide">Sakolee</div>
      <div class="fw-700 fs-22 text-2e" :class="$q.screen.width < 1024 ? '' : 'hidden'">
        Sakolee
      </div>
    </div>
    <div v-if="!menuCollapsed" class="tenant-section flex bg-primary" :class="menuCollapsed ? 'px-10' : 'px-20'">
      <q-avatar size="40px" class="tenant-logo" :class="tenantLogoPreviewUrl ? 'q-pr-xl' : ''">
        <img v-if="tenantLogoPreviewUrl" :src="tenantLogoPreviewUrl" alt="Tenant Logo">
        <q-icon v-else name="o_account_balance" class="text-white" size="22px" />
      </q-avatar>
      <div class="tenant-info q-mini-drawer-hide">
        <div class="tenant-name text-white">{{ tenantName }}
          <q-tooltip anchor="bottom middle" self="top middle">{{ tenantName }}</q-tooltip>
        </div>
      </div>
      <div class="tenant-info" :class="$q.screen.width < 1024 ? '' : 'hidden'">
        <div class="tenant-name text-white">{{ tenantName }}
          <q-tooltip anchor="bottom middle" self="top middle">{{ tenantName }}</q-tooltip>
        </div>
      </div>
    </div>

    <div v-else class="mini-tenant">
      <q-avatar size="40px" class="tenant-logo">
        <img v-if="tenantLogoPreviewUrl" :src="tenantLogoPreviewUrl" alt="Tenant Logo">
        <q-icon v-else name="business" size="24px" />
      </q-avatar>
    </div>
  </div>
</template>

<script setup>
import { onMounted, onBeforeUnmount, ref } from "vue";
import { mediaApi } from "services/api";
import { useAuthStore } from "stores/auth";

defineProps({ menuCollapsed: { type: Boolean, default: false } });

const authStore = useAuthStore();
const tenantName = ref("");
const tenantLogoPreviewUrl = ref(null);

const loadTenant = async () => {
  const tenant = authStore.user?.tenants?.[0];
  console.log("Tenant:", tenant);
  if (!tenant) {
    tenantName.value = "";
    return;
  }

  tenantName.value = tenant.name || "";
  const mediaId = tenant.tenantLogoMediaId;

  if (!mediaId) {
    tenantLogoPreviewUrl.value = null;
    return;
  }

  try {
    const blob = await mediaApi.content(mediaId);
    if (blob) {
      tenantLogoPreviewUrl.value = URL.createObjectURL(blob);
    } else {
      tenantLogoPreviewUrl.value = null;
    }
  } catch (error) {
    console.error("Failed to load tenant logo:", error);
    tenantLogoPreviewUrl.value = null;
  }
};

onMounted(() => {
  loadTenant();
});

onBeforeUnmount(() => {
  if (tenantLogoPreviewUrl.value?.startsWith("blob:")) {
    URL.revokeObjectURL(tenantLogoPreviewUrl.value);
  }
});
</script>

<style scoped lang="scss">
.aside-header {
  width: 100%;
  box-sizing: border-box;
}

.pa-20 {
  padding: 20px !important;
}

.pa-10 {
  padding: 10px !important;
}

/* Product */
.brand-section {
  display: flex;
  align-items: center;
  gap: 12px;
}

.brand-logo {
  width: 40px;
  height: 40px;
  min-width: 40px;
  border-radius: 12px;

  display: flex;
  align-items: center;
  justify-content: center;

  background: linear-gradient(
    135deg,
    var(--q-primary),
    #6366f1
  );

  color: white;
}

.brand-name {
  font-size: 22px;
  font-weight: 700;
  color: #2e2e38;
}

/* Tenant */
.tenant-section {
  display: flex;
  align-items: center;
  gap: 0px;
  border-top: 1px solid #eeeeee;
}

.tenant-logo {
  // background: #f1f3f6;
  flex-shrink: 0;
}

.tenant-logo img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.tenant-name {
  font-size: 14px;
  font-weight: 600;
  color: #252533;

  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.mini-tenant {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}

.tenant-info {
  min-width: 0;
  flex: 1;
}
</style>
