<template>
  <!-- 
    ============================================================
    View Family Relation Drawer Component
    ============================================================
  -->
  <app-form-drawer
    v-model="isOpen"
    title="View Family Relation"
    :saving="viewLoading"
    :save-label="''"
    :hide-save="true"
    @cancel="closeView"
  >
    <div class="q-gutter-md">
      <!-- Relation Name Field Display -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Relation Name
        </div>
        <div class="text-2e fs-4">
          {{ viewRelation.name }}
        </div>
      </div>

      <!-- Status Badge Display -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Status
        </div>
        <q-badge :color="viewRelation.active ? 'positive' : 'grey'">
          {{ viewRelation.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Audit Metadata: Created By -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Created By
        </div>
        <div class="text-2e fs-14">
          {{ viewRelation.createdBy || "—" }}
        </div>
      </div>

      <!-- Audit Metadata: Created On -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Created On
        </div>
        <div class="text-2e fs-14">
          {{ formatDate(viewRelation.createdOnUtc) }}
        </div>
      </div>

      <!-- Audit Metadata: Updated By -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Updated By
        </div>
        <div class="text-2e fs-14">
          {{ viewRelation.updatedBy || "—" }}
        </div>
      </div>

      <!-- Audit Metadata: Updated On -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Updated On
        </div>
        <div class="text-2e fs-14">
          {{ formatDate(viewRelation.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { familyRelationApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  relationId: {
    type: [String, Number],
    default: null
  }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const viewLoading = ref(false);

/*
 * Reactive state to store detailed family relation attributes for viewing.
 */
const viewRelation = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

/*
 * Two-way model binding wrapper for drawer open/close status.
 */
const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

/*
 * Watch drawer state changes and fetch detailed information on open.
 */
watch(
  () => props.modelValue,
  async (val) => {
    if (val && props.relationId) {
      viewLoading.value = true;
      try {
        const relation = await familyRelationApi.get(props.relationId);
        viewRelation.id = relation?.id;
        viewRelation.name = relation?.name || "";
        viewRelation.active = relation?.active ?? true;
        viewRelation.createdBy = relation?.createdBy || "";
        viewRelation.createdOnUtc = relation?.createdOnUtc || null;
        viewRelation.updatedBy = relation?.updatedBy || "";
        viewRelation.updatedOnUtc = relation?.updatedOnUtc || null;
      } catch (err) {
        isOpen.value = false;
        notify.error(getApiErrorMessage(err));
      } finally {
        viewLoading.value = false;
      }
    } else if (!val) {
      resetViewRelation();
    }
  }
);

/*
 * Reset view relation reactive state values.
 */
const resetViewRelation = () => {
  viewRelation.id = null;
  viewRelation.name = "";
  viewRelation.active = true;
  viewRelation.createdBy = "";
  viewRelation.createdOnUtc = null;
  viewRelation.updatedBy = "";
  viewRelation.updatedOnUtc = null;
};

/*
 * Close the view drawer safely.
 */
const closeView = () => {
  isOpen.value = false;
  resetViewRelation();
};

/*
 * Format date values to a localized readable format.
 */
const formatDate = (value) => {
  if (!value) {
    return "—";
  }
  return new Date(value).toLocaleString();
};
</script>