<template>
  <app-form-dialog v-model="dialogOpen" :title="form?.name ? `${form.name}` : 'View Tenant'" :subtitle="tenantAdministratorName" :avatar-url="previewUrl" size="lg" hide-save hide-footer>
    <div v-if="loading" class="view-student__loading">
      <q-spinner color="primary" size="32px" />
    </div>

    <div v-else class="view-student-scss">
      <!-- =====================================================
           TENANT INFORMATION + FAMILY
           ===================================================== -->
      <div class="row q-col-gutter-md">
        <!-- Tenant Information -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <span class="material-symbols-outlined fs-16">domain</span>
              <span class="text-4d fw-700 fs-12 lh-16">TENANT INFORMATION</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Tenant Name</span>
                <span class="fs-11 fw-600 text-2e"> {{ form.name || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Identifier</span>
                <span class="fs-11 fw-600 text-2e">{{ form.identifier || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Time Zone</span>
                <span class="fs-11 fw-600 text-2e">{{ form.timeZoneId || "—" }}</span>
              </div>
            </div>
          </section>
          <section class="border-80 br-16 pa-20 q-mt-md">
            <div class="info-card__header">
              <span class="material-symbols-outlined fs-16">admin_panel_settings</span>
              <span class="text-4d fw-700 fs-12 lh-16">TENANT ADMINISTRATOR DETAILS</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Admin First Name</span>
                <span class="fs-11 fw-600 text-2e"> {{ form.administrator?.firstName || "—" }}
                </span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Admin Last Name</span>
                <span class="fs-11 fw-600 text-2e"> {{ form.administrator?.lastName || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Admin Email</span>
                <span class="fs-11 fw-600 text-2e"> {{ form.administrator?.email || "—" }}</span>
              </div>
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Admin Phone Number</span>
                <span class="fs-11 fw-600 text-2e">  {{ form.administrator?.phoneNumber || "—" }}</span>
              </div>
            </div>
          </section>
        </div>

        <!-- Tenant Address Information -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <!-- <q-icon name="o_groups" class="fs-16 text-4d" /> -->
              <span class="material-symbols-outlined fs-16">location_on</span>
              <span class="text-4d fw-700 fs-12 lh-16">TENANT ADDRESS INFORMATION</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Country</span>
                <span class="fs-11 fw-600 text-d4">
                  {{ form.address?.countryName || "—" }}
                </span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">State/Province</span>
                <span class="fs-11 fw-600 text-2ee">
                  {{ form.address?.stateName || "—" }}
                </span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">City</span>
                <span class="fs-11 fw-600 text-2e">
                  {{ form.address?.cityName || "—" }}
                </span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Zip Code</span>
                <span class="fs-11 fw-600 text-2e">
                  {{ form.address?.postalCode || "—" }}
                </span>
              </div>
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Address1</span>
                <span class="fs-11 fw-600 text-2e">
                  {{ form.address?.addressLine1 || "—" }}
                </span>
              </div>
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Address2</span>
                <span class="fs-11 fw-600 text-2e">
                  {{ form.address?.addressLine2 || "—" }}
                </span>
              </div>
            </div>
          </section>
          <!-- <section class="border-80 br-16 pa-20 q-mt-md">
            <div class="info-card__header">
              <q-icon name="o_person_outline" class="fs-16 text-4d" />
              <span class="text-4d fw-700 fs-12 lh-16">TENANT LOGO</span>
            </div>

            <div v-if="previewUrl" class="mt-12">
              <div class="tenant-logo-wrapper">
                <q-avatar size="72px" class="tenant-logo">
                  <img :src="previewUrl" alt="Tenant Logo">
                </q-avatar>
              </div>
            </div>
          </section> -->
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>
<script setup>
import { computed, onBeforeUnmount, ref, watch } from "vue";
import {
  tenantApi,
  mediaApi,
  getApiErrorMessage
} from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDialog from "components/common/AppFormDialog.vue";

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },

  id: {
    type: [String, Number],
    default: null
  }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();

const dialogOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value)
});

const loading = ref(false);

const form = ref(null);

const previewUrl = ref(null);

const revokePreviewUrl = () => {
  if (previewUrl.value?.startsWith("blob:")) {
    URL.revokeObjectURL(previewUrl.value);
  }

  previewUrl.value = null;
};

const resetForm = () => {
  form.value = null;
  revokePreviewUrl();
};

const loadTenant = async () => {
  if (!props.id) {
    resetForm();
    return;
  }

  loading.value = true;

  try {
    const response = await tenantApi.get(props.id);

    form.value = response;

    console.log("Tenant data loaded:", response);

    // Tenant logo
    if (response?.tenantLogoMediaId) {
      const blob = await mediaApi.content(
        response.tenantLogoMediaId
      );

      if (blob) {
        revokePreviewUrl();
        previewUrl.value = URL.createObjectURL(blob);
      }
    }
  } catch (err) {
    resetForm();
    notify.error(getApiErrorMessage(err));
    dialogOpen.value = false;
  } finally {
    loading.value = false;
  }
};

const tenantAdministratorName = computed(() => {
  const administrator = form.value?.administrator;

  if (!administrator) {
    return "Tenant Administrator";
  }

  const fullName = [
    administrator.firstName,
    administrator.lastName
  ]
    .filter(Boolean)
    .join(" ")
    .trim();

  return fullName || "Tenant Administrator";
});

watch(
  () => props.modelValue,
  async (value) => {
    if (value) {
      await loadTenant();
    } else {
      resetForm();
    }
  }
);

watch(
  () => props.id,
  async () => {
    if (props.modelValue) {
      await loadTenant();
    }
  }
);

onBeforeUnmount(() => {
  revokePreviewUrl();
});
</script>
