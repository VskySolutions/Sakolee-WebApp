export default [
  {
    path: "/t-shirt-sizes",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "t_shirt_sizes",
        component: () =>
          import("modules/t-shirt-size/pages/index.vue"),
        meta: {
          requiresAuth: true,
          permissions: ["tShirtSizes.read"],
          title: "T-Shirt Sizes"
        }
      }
    ]
  }
];
