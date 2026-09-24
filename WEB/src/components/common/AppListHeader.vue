<template>
  <q-card flat class="app-list-header q-mb-md">

    <div class="app-list-header__layout">

      <!-- =====================================================
           TOP LEFT CONTENT
           Breadcrumb
           Title
           Description
           ===================================================== -->
      <div class="app-list-header__top">

        <!-- Breadcrumb -->
        <div
          v-if="breadcrumbs.length"
          class="app-list-header__breadcrumbs"
        >
          <app-breadcrumbs
            :items="breadcrumbs"
            no-margin
            class="app-list-header__crumbs"
          />
        </div>

        <!-- Title -->
        <div
          v-if="title"
          class="app-list-header__title q-mb-xs"
        >
          {{ title }}
        </div>

        <!-- Description -->
        <div
          v-if="description"
          class="app-list-header__description"
        >
          {{ description }}
        </div>

      </div>

      <!-- =====================================================
           RIGHT SIDE ACTIONS
           Centered against Breadcrumb + Title + Description
           ===================================================== -->
      <div class="app-list-header__actions">

        <!-- Filter -->
        <q-btn
          v-if="showFilters"
          flat
          no-caps
          icon="o_filter_list"
          label="Advance Filter"
          class="br-12 page-top-btns grey-border"
          padding="8px 12px"
          @click="$emit('filters')"
        >
          <q-badge
            v-if="filterCount"
            floating
            color="primary"
          >
            {{ filterCount }}
          </q-badge>
        </q-btn>

        <!-- Page specific actions -->
        <slot name="actions" />

        <!-- Add -->
        <q-btn
          v-if="showAdd"
          unelevated
          no-caps
          color="primary"
          :icon="addIcon"
          :label="addLabel"
          :disable="addDisable"
          class="br-12 page-top-btns"
          padding="8px 12px"
          @click="$emit('add')"
        />

        <!-- Back -->
        <q-btn
          v-if="showBack"
          flat
          no-caps
          icon="o_arrow_back"
          label="Back"
          class="br-12 page-top-btns grey-border"
          padding="8px 12px"
          @click="$emit('back')"
        />

      </div>

      <!-- =====================================================
           SEARCH
           Always below Description
           ===================================================== -->
      <q-input
        v-if="showSearch"
        :model-value="search"
        dense
        outlined
        debounce="300"
        :placeholder="searchPlaceholder"
        class="app-list-header__search"
        @update:model-value="$emit('update:search', $event)"
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>

    </div>

  </q-card>
</template>
<script setup>
import AppBreadcrumbs from "components/common/AppBreadcrumbs.vue";

defineProps({
  breadcrumbs: {
    type: Array,
    default: () => []
  },

  title: {
    type: String,
    default: ""
  },

  description: {
    type: String,
    default: ""
  },

  search: {
    type: String,
    default: ""
  },

  searchPlaceholder: {
    type: String,
    default: "Search"
  },

  showSearch: {
    type: Boolean,
    default: false
  },

  showFilters: {
    type: Boolean,
    default: false
  },

  filterCount: {
    type: Number,
    default: 0
  },

  showAdd: {
    type: Boolean,
    default: false
  },

  addLabel: {
    type: String,
    default: "Add"
  },

  addIcon: {
    type: String,
    default: "o_add"
  },

  addDisable: {
    type: Boolean,
    default: false
  },

  showBack: {
    type: Boolean,
    default: false
  }
});

defineEmits([
  "update:search",
  "filters",
  "add",
  "back"
]);
</script>

<style scoped>
/* ============================================================
   Main Card
   ============================================================ */

.app-list-header {
  border-radius: 12px;
}

/* ============================================================
   Main Layout
   ============================================================ */

.app-list-header__layout {
  display: grid;

  /*
   * Column 1:
   * Breadcrumb + Title + Description + Search
   *
   * Column 2:
   * Actions
   */
  grid-template-columns: minmax(0, 1fr) auto;

  /*
   * Row 1:
   * Breadcrumb + Title + Description
   *
   * Row 2:
   * Search
   */
  grid-template-rows: auto auto;

  column-gap: 32px;
  row-gap: 16px;
}

/* ============================================================
   TOP LEFT
   ============================================================ */

.app-list-header__top {
  grid-column: 1;
  grid-row: 1;

  min-width: 0;

  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

/* ============================================================
   Breadcrumb
   ============================================================ */

.app-list-header__breadcrumbs {
  width: 100%;
  min-width: 0;
}

/* ============================================================
   Title
   ============================================================ */

.app-list-header__title {
  font-size: 24px;
  line-height: 1.2;
  font-weight: 700;

  color: var(--q-dark);
}

/* ============================================================
   Description
   ============================================================ */

.app-list-header__description {
  font-size: 13px;
  line-height: 1.5;

  color: var(--q-secondary);

  max-width: 850px;
}

/* ============================================================
   RIGHT SIDE ACTIONS
   ============================================================ */

.app-list-header__actions {
  grid-column: 2;
  grid-row: 1;

  align-self: center;

  display: flex;
  align-items: center;
  justify-content: flex-end;

  flex-wrap: wrap;

  gap: 8px;

  min-width: 0;
}

/* ============================================================
   SEARCH
   ============================================================ */

.app-list-header__search {
  grid-column: 1;
  grid-row: 2;

  width: 320px;
  max-width: 100%;

  margin-top: 0;
}

/* ============================================================
   TABLET
   ============================================================ */

@media (max-width: 1024px) {

  .app-list-header__layout {
    column-gap: 20px;
  }

  .app-list-header__search {
    width: 280px;
  }

  .app-list-header__actions {
    max-width: 320px;
  }
}

/* ============================================================
   MOBILE
   ============================================================ */

@media (max-width: 767px) {

  .app-list-header__layout {
    grid-template-columns: 1fr;

    grid-template-rows: auto auto auto;

    gap: 16px;

    padding: 14px;
  }

  .app-list-header__top {
    grid-column: 1;
    grid-row: 1;

    width: 100%;
  }

  .app-list-header__actions {
    grid-column: 1;
    grid-row: 2;

    width: 100%;

    justify-content: flex-start;

    max-width: none;
  }

  .app-list-header__search {
    grid-column: 1;
    grid-row: 3;

    width: 100%;
  }

  .app-list-header__title {
    margin-top: 12px;

    font-size: 22px;
  }

  .app-list-header__description {
    max-width: 100%;
  }
}

/* ============================================================
   SMALL PHONE
   ============================================================ */

@media (max-width: 599px) {

  .app-list-header__actions {
    display: grid;

    grid-template-columns: 1fr 1fr;

    width: 100%;
  }

  .app-list-header__actions :deep(.q-btn) {
    width: 100%;
  }
}
</style>
