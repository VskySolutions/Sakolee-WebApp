export default [
  {
    path: "/locations",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "locations",
        component: () => import("modules/location/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["locations.read"], title: "Locations" }
      }
    ]
  }
];
