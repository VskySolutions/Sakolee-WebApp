<template>
  <q-page class="sakolee-marketing-page">
    <div class="scroll-progress" :style="{ width: `${scrollProgress}%` }" />
    <div class="cursor-light" aria-hidden="true" />

    <!-- HERO -->
    <section id="top" class="hero">
      <div class="hero-aurora hero-aurora-one" />
      <div class="hero-aurora hero-aurora-two" />
      <div class="hero-grid" />

      <div class="hero-beams" aria-hidden="true">
        <i /><i /><i /><i />
      </div>

      <div class="hero-radar" aria-hidden="true">
        <i /><i /><i /><span />
      </div>

      <div class="hero-inner">
        <div class="hero-copy">
          <div class="eyebrow enter enter-one">
            <span class="eyebrow-pulse" />
            All-in-one dance studio
          </div>

          <h1 class="enter enter-two">
            Run your studio.
            <span>Not spreadsheets.</span>
          </h1>

          <p class="hero-text enter enter-three">
            Sakolee handles enrollment, classes, recitals, costumes, and email automation — so you can focus on what you love: dance.
          </p>

          <div class="hero-actions enter enter-four">
            <a class="button button-primary" href="#pricing">
              Start free trial
              <svg viewBox="0 0 20 20" aria-hidden="true"><path d="M4 10h11M11 6l4 4-4 4" /></svg>
            </a>
            <a class="button button-ghost" href="#preview">
              <svg viewBox="0 0 20 20" aria-hidden="true"><path d="m7.5 5 7 5-7 5V5Z" /></svg>
              Watch 2-min demo
            </a>
          </div>

          <div class="hero-proof enter enter-five">
            <div class="avatar-stack" aria-hidden="true">
              <span>SM</span><span>KL</span><span>RT</span>
            </div>
            <div>
              <div class="stars" aria-label="5 out of 5 stars">★★★★★</div>
              <p>Real results, real studios</p>
            </div>
          </div>
        </div>

        <div class="crm-stage enter enter-three" aria-label="Sakolee dashboard preview">
          <div class="orbit orbit-one" />
          <div class="orbit orbit-two" />

          <div class="product-hero-scene">
            <div class="product-halo" />
            <img class="product-shot shot-dashboard" src="~assets/dashboard.png" alt="Sakolee studio dashboard">
            <img class="product-shot shot-onboarding" src="~assets/onboarding.png" alt="Automated onboarding interface">
            <img class="product-shot shot-billing" src="~assets/billing.png" alt="Billing interface">
            <img class="product-shot shot-schedule" src="~assets/schedule.png" alt="Studio schedule interface">
            <span class="scene-particle particle-one" />
            <span class="scene-particle particle-two" />
            <span class="scene-particle particle-three" />
          </div>
        </div>
      </div>

      <div class="hero-ticker" aria-hidden="true">
        <div>
          <span>Student enrollment</span><i />
          <span>Enrollment Operations &amp; Tasks Management</span><i />
          <span>Class management</span><i />
          <span>Recital management</span><i />
          <span>Costume library and inventory</span><i />
          <span>Email automation</span><i />
          <span>Student enrollment</span><i />
          <span>Enrollment Operations &amp; Tasks Management</span><i />
        </div>
      </div>
    </section>

    <!-- FEATURES -->
    <!-- <section id="features" class="section features-section">
      <div class="section-shell">
        <div class="section-heading reveal">
          <div>
            <p class="section-kicker"><span />Everything in one place</p>
            <h2>Every module your<br>studio needs</h2>
          </div>
          <p>Built from the ground up for dance studios — not retrofitted from generic business software.</p>
        </div>

        <div class="feature-grid">
          <article
            v-for="feature in features"
            :key="feature.number"
            :class="['feature-card', feature.layout, { accent: feature.accent }, 'reveal']"
            :style="{ '--reveal-delay': feature.delay }"
          >
            <div class="feature-card-head">
              <span class="feature-number">{{ feature.number }}</span>
              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true"><path d="M4 10h11M11 6l4 4-4 4" /></svg>
              </span>
            </div>

            <div :class="['feature-visual', `visual-${feature.number}`]" aria-hidden="true">
              <span class="visual-orbit"><i /></span>
              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />
              <span class="visual-path path-one" />
              <span class="visual-path path-two" />
              <span class="visual-bars"><i /><i /><i /><i /></span>
            </div>
            <h3>{{ feature.title }}</h3>
            <p>{{ feature.description }}</p>
            <ul>
              <li v-for="item in feature.bullets" :key="item">
                <svg viewBox="0 0 20 20" aria-hidden="true"><path d="m4 10.5 3.5 3.5L16 6" /></svg>
                {{ item }}
              </li>
            </ul>
          </article>
        </div>
      </div>
    </section> -->

    <!-- FEATURES -->
    <section id="features" class="section features-section">
      <div class="section-shell">

        <div class="section-heading reveal">
          <div>
            <p class="section-kicker">
              <span />
              Everything in one place
            </p>

            <h2>Every module your<br>studio needs</h2>
          </div>

          <p>
            Built from the ground up for dance studios — not retrofitted from
            generic business software.
          </p>
        </div>

        <div class="feature-grid">

          <!-- 01 Student Enrollment -->
          <article class="feature-card wide reveal">
            <div class="feature-card-head">
              <span class="feature-number">01</span>

              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M4 10h11M11 6l4 4-4 4" />
                </svg>
              </span>
            </div>

            <div class="feature-visual visual-1" aria-hidden="true">
              <span class="visual-orbit">
                <i />
              </span>

              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />

              <span class="visual-path path-one" />
              <span class="visual-path path-two" />

              <span class="visual-bars">
                <i />
                <i />
                <i />
                <i />
              </span>
            </div>

            <h3>Student enrollment</h3>

            <p>
              New students, transfers, drops, and trial programs — with
              per-student onboarding task checklists and a full activity history
              for every record.
            </p>

            <ul>
              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                New student onboarding projects
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Class transfer management
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Trial student pipeline
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Drop request tracking
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Wait list management
              </li>
            </ul>
          </article>

          <!-- 02 Enrollment Operations -->
          <article class="feature-card wide accent reveal">
            <div class="feature-card-head">
              <span class="feature-number">02</span>

              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M4 10h11M11 6l4 4-4 4" />
                </svg>
              </span>
            </div>

            <div class="feature-visual visual-2" aria-hidden="true">
              <span class="visual-orbit"><i /></span>
              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />
              <span class="visual-path path-one" />
              <span class="visual-path path-two" />
              <span class="visual-bars"><i /><i /><i /><i />
              </span>
            </div>

            <h3>Enrollment Operations &amp; Tasks Management</h3>

            <p>
              Every onboarding, enrollment, and drop request runs as a structured
              project with auto-generated tasks — so nothing slips through the
              cracks.
            </p>

            <ul>
              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Onboarding, enrollment, and drop project types
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Automatic task generation per project
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Manual task creation and editing
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Progress monitoring by status and date
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Notes and full task history
              </li>
            </ul>
          </article>

          <!-- 03 Class Management -->
          <article class="feature-card standard reveal">
            <div class="feature-card-head">
              <span class="feature-number">03</span>

              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M4 10h11M11 6l4 4-4 4" />
                </svg>
              </span>
            </div>

            <div class="feature-visual visual-3" aria-hidden="true">
              <span class="visual-orbit">
                <i />
              </span>

              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />

              <span class="visual-path path-one" />
              <span class="visual-path path-two" />

              <span class="visual-bars">
                <i />
                <i />
                <i />
                <i />
              </span>
            </div>

            <h3>Class management</h3>

            <p>
              Schedules, instructors, locations, and real-time capacity — kept
              accurate and visible to everyone on your team, always.
            </p>

            <ul>
              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Class schedules and sessions
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Instructor assignments
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Capacity and enrollment tracking
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Waiting list management
              </li>
            </ul>
          </article>

          <!-- 04 Recital Management -->
          <article class="feature-card standard reveal">
            <div class="feature-card-head">
              <span class="feature-number">04</span>

              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M4 10h11M11 6l4 4-4 4" />
                </svg>
              </span>
            </div>

            <div class="feature-visual visual-4" aria-hidden="true">
              <span class="visual-orbit">
                <i />
              </span>

              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />

              <span class="visual-path path-one" />
              <span class="visual-path path-two" />

              <span class="visual-bars">
                <i />
                <i />
                <i />
                <i />
              </span>
            </div>

            <h3>Recital management</h3>

            <p>
              Themes, costumes, music, lighting cues, participant tracking, and
              volunteer approvals — organized end to end for every show.
            </p>

            <ul>
              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Performance sequences and themes
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Music and lights cue library
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Participant tracking
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Costume form approvals
              </li>
            </ul>
          </article>

          <!-- 05 Costume Library -->
          <article class="feature-card standard reveal">
            <div class="feature-card-head">
              <span class="feature-number">05</span>

              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M4 10h11M11 6l4 4-4 4" />
                </svg>
              </span>
            </div>

            <div class="feature-visual visual-5" aria-hidden="true">
              <span class="visual-orbit">
                <i />
              </span>

              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />

              <span class="visual-path path-one" />
              <span class="visual-path path-two" />

              <span class="visual-bars">
                <i />
                <i />
                <i />
                <i />
              </span>
            </div>

            <h3>Costume library and inventory</h3>

            <p>
              Every costume record in one place — associated to categories,
              materials or measurements, and tracked through stock and handover
              status all season.
            </p>

            <ul>
              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Costume records with details and accessories
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Allocation of costumes to participants
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Student measurement import in Excel
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Inventory quantities and stock movements
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Handover tracking and status history
              </li>
            </ul>
          </article>

          <!-- 06 Costume Order -->
          <article class="feature-card wide reveal">
            <div class="feature-card-head">
              <span class="feature-number">06</span>

              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M4 10h11M11 6l4 4-4 4" />
                </svg>
              </span>
            </div>

            <div class="feature-visual visual-6" aria-hidden="true">
              <span class="visual-orbit">
                <i />
              </span>

              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />

              <span class="visual-path path-one" />
              <span class="visual-path path-two" />

              <span class="visual-bars">
                <i />
                <i />
                <i />
                <i />
              </span>
            </div>

            <h3>Costume order management</h3>

            <p>
              The full procurement cycle from order creation to delivery, plus a
              dedicated workflow for returns — vendors, pricing, and status tracked
              end to end.
            </p>

            <ul>
              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Order creation with vendor and pricing details
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Order status tracking from creation to delivery
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Edit, delete, and view full order history
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Return requests with reason and status tracking
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Search and filter across orders and returns
              </li>
            </ul>
          </article>

          <!-- 07 Email Automation -->
          <article class="feature-card wide reveal">
            <div class="feature-card-head">
              <span class="feature-number">07</span>

              <span class="feature-arrow">
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M4 10h11M11 6l4 4-4 4" />
                </svg>
              </span>
            </div>

            <div class="feature-visual visual-7" aria-hidden="true">
              <span class="visual-orbit">
                <i />
              </span>

              <span class="visual-node node-one" />
              <span class="visual-node node-two" />
              <span class="visual-node node-three" />

              <span class="visual-path path-one" />
              <span class="visual-path path-two" />

              <span class="visual-bars">
                <i />
                <i />
                <i />
                <i />
              </span>
            </div>

            <h3>Email automation</h3>

            <p>
              Reusable templates, scheduled and automated sends, and
              per-recipient delivery logs — for students, parents, and staff.
            </p>

            <ul>
              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Template library with attachments
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Manual and automated scheduling
              </li>

              <li>
                <svg viewBox="0 0 20 20" aria-hidden="true">
                  <path d="m4 10.5 3.5 3.5L16 6" />
                </svg>
                Delivery logs and status tracking
              </li>
            </ul>
          </article>

        </div>
      </div>
    </section>

    <!-- LIVE PREVIEW -->
    <section id="preview" class="section preview-section">
      <div class="preview-glow" />
      <div class="section-shell">
        <div class="section-heading centered reveal">
          <div>
            <p class="section-kicker"><span />Live preview</p>
            <h2>See Sakolee in action</h2>
          </div>
          <p>Click any module to explore what your team sees every day.</p>
        </div>

        <div class="preview-tabs reveal" role="tablist" aria-label="Sakolee modules">
          <button
            v-for="(item, index) in previewData"
            :key="item.index"
            type="button"
            role="tab"
            :aria-selected="activePreview === index"
            :class="{ active: activePreview === index }"
            @click="activePreview = index"
          >
            {{ item.tab }}
          </button>
        </div>

        <div class="preview-workspace reveal">
          <div class="preview-copy">
            <span class="preview-index">{{ currentPreview.index }}</span>
            <h3>{{ currentPreview.title }}</h3>
            <p>{{ currentPreview.description }}</p>
            <ul>
              <li v-for="bullet in currentPreview.bullets" :key="bullet">
                <svg viewBox="0 0 20 20" aria-hidden="true"><path d="m4 10.5 3.5 3.5L16 6" /></svg>
                {{ bullet }}
              </li>
            </ul>
          </div>

          <div class="preview-app">
            <div class="enrollment-panel module-panel">
              <div class="enrollment-panel-head">
                <strong>{{ currentPreview.head }}</strong>
                <span>{{ currentPreview.status }}</span>
              </div>
              <div :key="currentPreview.index" class="enrollment-list">
                <div
                  v-for="(row, rowIndex) in currentPreview.rows"
                  :key="row[0]"
                  class="enrollment-row"
                  :style="{ '--row-delay': `${rowIndex * 0.08}s` }"
                >
                  <div>
                    <strong>{{ row[0] }}</strong>
                    <small>{{ row[1] }}</small>
                  </div>
                  <em :class="row[3]">{{ row[2] }}</em>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- RESOURCES / TESTIMONIALS -->
    <section id="resources" class="section resources-section">
      <div class="section-shell">
        <div class="workflow-grid">
          <div class="workflow-panel reveal">
            <p class="section-kicker"><span />Here’s how</p>
            <h2>Up and running in one week</h2>
            <div class="steps">
              <div v-for="step in steps" :key="step.number" class="step">
                <span>{{ step.number }}</span>
                <div>
                  <h3>{{ step.title }}</h3>
                  <p>{{ step.text }}</p>
                </div>
              </div>
            </div>
          </div>

          <blockquote class="featured-quote reveal">
            <p class="section-kicker"><span />What studios say</p>
            <h2>Real results, real studios</h2>
            <div class="quote-stars">★★★★★</div>
            <p class="quote-copy">“Honestly, it’s probably some of the few good tools for dance that is dance specific. Nice user experience. And the comprehensive features everything before now.”</p>
            <footer>
              <span>SM</span>
              <div><strong>Sarah M., Studio Director</strong><small>17-year established dance studio</small></div>
            </footer>
          </blockquote>
        </div>

        <div class="testimonial-grid">
          <blockquote v-for="item in testimonials" :key="item.name" class="testimonial-card reveal" :style="{ '--reveal-delay': item.delay }">
            <div class="quote-stars">★★★★★</div>
            <p>“{{ item.quote }}”</p>
            <footer>
              <span>{{ item.initials }}</span>
              <div><strong>{{ item.name }}</strong><small>{{ item.role }}</small></div>
            </footer>
          </blockquote>
        </div>
      </div>
    </section>

    <!-- PRICING -->
    <section id="pricing" class="section pricing-section">
      <div class="pricing-orb" />
      <div class="section-shell">
        <div class="section-heading reveal">
          <div>
            <p class="section-kicker"><span />Pricing</p>
            <h2>Simple, transparent pricing</h2>
          </div>
          <p>No setup fees. No contracts. Cancel anytime.</p>
        </div>

        <div class="pricing-grid">
          <article v-for="plan in plans" :key="plan.name" :class="['price-card', { featured: plan.featured }, 'reveal']">
            <span v-if="plan.featured" class="popular-pill">Most popular</span>
            <span class="price-label">{{ plan.name }}</span>
            <h3>
              <strong>{{ plan.price }}</strong>
              <small v-if="plan.period">{{ plan.period }}</small>
            </h3>
            <p>{{ plan.description }}</p>
            <ul>
              <li v-for="item in plan.features" :key="item">
                <svg viewBox="0 0 20 20" aria-hidden="true"><path d="m4 10.5 3.5 3.5L16 6" /></svg>
                {{ item }}
              </li>
            </ul>
            <a :class="['price-button', { primary: plan.featured }]" href="#demo">
              {{ plan.cta }}
              <svg viewBox="0 0 20 20" aria-hidden="true"><path d="M4 10h11M11 6l4 4-4 4" /></svg>
            </a>
          </article>
        </div>
      </div>
    </section>

    <!-- CTA -->
    <section id="demo" class="cta-section">
      <div class="cta-card reveal">
        <div class="cta-grid" />
        <div>
          <p class="section-kicker"><span />Sakolee</p>
          <h2>Give your studio the system it <em>actually needs.</em></h2>
          <p>Join dance studios already running smoother, calmer operations with Sakolee.</p>
        </div>
        <div class="cta-actions cta-actions-btns">
          <a class="button button-primary q-pa-md br-12 fw-700 border-price flex justify-center items-center" href="#top">
            Book a free demo
            <svg viewBox="0 0 20 20" class="q-ml-sm" aria-hidden="true"><path d="M4 10h11M11 6l4 4-4 4" /></svg>
          </a>
          <a class="button button-ghost text-center q-pa-md br-12 fw-700 border-price" href="#pricing">View pricing</a>
        </div>
      </div>
    </section>

    <!-- FOOTER -->
    <footer class="footer">
      <div>
        <a href="#top" aria-label="Sakolee home">
          <span class="brand footer-brand">
            <span class="brand-mark" aria-hidden="true"><i /><i /><i /></span>
            <span class="fs-16 fw-800 q-pt-xs">Sakolee</span>
          </span>
        </a>
        <p class="q-pt-xs">© 2026 Sakolee · Built for dance studios</p>
      </div>
    </footer>
  </q-page>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from "vue";

const activePreview = ref(0);
const scrollProgress = ref(0);
const currentPreview = computed(() => previewData[activePreview.value]);

const updateScrollProgress = () => {
  const doc = document.documentElement;
  scrollProgress.value = ((doc.scrollTop || window.scrollY) / ((doc.scrollHeight - doc.clientHeight) || 1)) * 100;
};

let observer;

onMounted(() => {
  window.addEventListener("scroll", updateScrollProgress, { passive: true });
  window.addEventListener("mousemove", (event) => {
    document.documentElement.style.setProperty("--pointer-x", `${event.clientX}px`);
    document.documentElement.style.setProperty("--pointer-y", `${event.clientY}px`);
  }, { passive: true });

  updateScrollProgress();

  observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) entry.target.classList.add("is-visible");
    });
  }, { threshold: 0.12 });

  document.querySelectorAll(".sakolee-marketing-page .reveal").forEach((element) => observer.observe(element));
});

onBeforeUnmount(() => {
  window.removeEventListener("scroll", updateScrollProgress);
  observer?.disconnect();
});

// const features = [
//   { number: "01", title: "Student enrollment", layout: "wide", delay: "0s", description: "New students, transfers, drops, and trial programs — with per-student onboarding task checklists and a full activity history for every record.", bullets: ["New student onboarding projects", "Class transfer management", "Trial student pipeline", "Drop request tracking", "Wait list management"] },
//   { number: "02", title: "Enrollment Operations & Tasks Management", layout: "wide", accent: true, delay: ".08s", description: "Every onboarding, enrollment, and drop request runs as a structured project with auto-generated tasks — so nothing slips through the cracks.", bullets: ["Onboarding, enrollment, and drop project types", "Automatic task generation per project", "Manual task creation and editing", "Progress monitoring by status and date", "Notes and full task history"] },
//   { number: "03", title: "Class management", layout: "standard", delay: ".16s", description: "Schedules, instructors, locations, and real-time capacity — kept accurate and visible to everyone on your team, always.", bullets: ["Class schedules and sessions", "Instructor assignments", "Capacity and enrollment tracking", "Waiting list management"] },
//   { number: "04", title: "Recital management", layout: "standard", delay: "0s", description: "Themes, costumes, music, lighting cues, participant tracking, and volunteer approvals — organized end to end for every show.", bullets: ["Performance sequences and themes", "Music and lights cue library", "Participant tracking", "Costume form approvals"] },
//   { number: "05", title: "Costume library and inventory", layout: "standard", delay: ".08s", description: "Every costume record in one place — associated to categories, materials or measurements, and tracked through stock and handover status all season.", bullets: ["Costume records with details and accessories", "Allocation of costumes to participants", "Student measurement import in Excel", "Inventory quantities and stock movements", "Handover tracking and status history"] },
//   { number: "06", title: "Costume order management", layout: "wide", delay: ".16s", description: "The full procurement cycle from order creation to delivery, plus a dedicated workflow for returns — vendors, pricing, and status tracked end to end.", bullets: ["Order creation with vendor and pricing details", "Order status tracking from creation to delivery", "Edit, delete, and view full order history", "Return requests with reason and status tracking", "Search and filter across orders and returns"] },
//   { number: "07", title: "Email automation", layout: "wide", delay: "0s", description: "Reusable templates, scheduled and automated sends, and per-recipient delivery logs — for students, parents, and staff.", bullets: ["Template library with attachments", "Manual and automated scheduling", "Delivery logs and status tracking"] }
// ];

const previewData = [
  { index: "01", tab: "Onboarding automation", title: "Enrollment Operations & Tasks Management", description: "Every onboarding, enrollment, and drop request runs as a project. The moment one starts, Sakolee generates its task checklist automatically and assigns each task to the right staff member — no manual setup required.", bullets: ["Three project categories: onboarding, enrollment, and drop", "Tasks auto-generated the moment a project is created", "Automatic assignment to staff based on role", "Manual task creation for anything outside the template", "Progress monitoring by status, student, project, and date", "Notes and complete activity history on every task"], head: "Sofia M. — onboarding project", status: "4 of 6 tasks done", rows: [["Welcome call", "Auto-assigned · Front desk", "Complete", "complete"], ["Enrollment paperwork", "Auto-assigned · Front desk", "Complete", "complete"], ["Measurements collected", "Auto-assigned · Instructor", "Complete", "complete"], ["Class placement confirmed", "Auto-assigned · Admin", "Complete", "complete"], ["Welcome packet sent", "Auto-assigned · Front desk", "In progress", "in-progress"], ["First class check-in", "Manual task · Instructor", "Scheduled", "scheduled"]] },
  { index: "02", tab: "Enrollment", title: "Student enrollment", description: "Manage new students, transfers, trials, drops, and waitlists from one place, with a clear view of each student journey.", bullets: ["New student onboarding projects", "Class transfer management", "Trial student pipeline", "Drop request tracking", "Wait list management"], head: "Student enrollment", status: "24 active students", rows: [["Maya R.", "New student · Trial", "New", "in-progress"], ["Ava K.", "Transfer request · Ballet", "Pending", "scheduled"], ["Sofia M.", "Onboarding · Level 2", "Active", "complete"], ["Liam P.", "Wait list · Hip Hop", "Waiting", "scheduled"], ["Emma T.", "Drop request", "Review", "in-progress"]] },
  { index: "03", tab: "Recitals", title: "Recital management", description: "Keep recital themes, performances, music, lighting cues, participants, costumes, and approvals organized from planning through show day.", bullets: ["Performance sequences and themes", "Music and lights cue library", "Participant tracking", "Costume form approvals"], head: "Spring recital — performance board", status: "18 performances", rows: [["Opening number", "Music + lighting cues", "Ready", "complete"], ["Junior Jazz", "24 participants · Costumes", "Approved", "complete"], ["Contemporary", "Music cue · 02:14", "In review", "in-progress"], ["Finale", "Full cast · Volunteer check", "Scheduled", "scheduled"], ["Costume approvals", "36 of 42 approved", "In progress", "in-progress"]] },
  { index: "04", tab: "Costume library", title: "Costume library and inventory", description: "Track costume records, participant allocations, measurements, quantities, stock movements, and handovers throughout the season.", bullets: ["Costume records with details and accessories", "Allocation of costumes to participants", "Student measurement import in Excel", "Inventory quantities and stock movements", "Handover tracking and status history"], head: "Costume inventory", status: "128 items tracked", rows: [["Pink Ballet Dress", "24 in stock · 18 allocated", "In stock", "complete"], ["Classic Tutu", "12 in stock · 12 allocated", "Allocated", "in-progress"], ["Jazz Jacket", "8 in stock · 2 allocated", "Low stock", "scheduled"], ["Show Shoes", "31 pairs · Mixed sizes", "In stock", "complete"], ["Handover batch #18", "16 of 20 handed over", "In progress", "in-progress"]] },
  { index: "05", tab: "Costume orders", title: "Costume order management", description: "Follow costume procurement from order creation through delivery and returns, with vendors, pricing, statuses, and history visible to the team.", bullets: ["Order creation with vendor and pricing details", "Order status tracking from creation to delivery", "Edit, delete, and view full order history", "Return requests with reason and status tracking", "Search and filter across orders and returns"], head: "Costume orders", status: "7 open orders", rows: [["Order #1048", "DanceWear Co. · $1,280", "Delivered", "complete"], ["Order #1051", "Stage Threads · $860", "Ordered", "in-progress"], ["Order #1053", "Motion Costumes · $540", "Awaiting delivery", "scheduled"], ["Order #1054", "DanceWear Co. · $420", "Return requested", "in-progress"], ["Order #1056", "Stage Threads · $1,120", "Draft", "scheduled"]] },
  { index: "06", tab: "Email automation", title: "Email automation", description: "Create reusable templates, schedule automated sends, and keep a delivery history for students, parents, and staff.", bullets: ["Template library with attachments", "Manual and automated scheduling", "Delivery logs and status tracking"], head: "Email automation", status: "96% delivered", rows: [["Welcome email", "New student workflow · 14 recipients", "Sent", "complete"], ["Class reminder", "Scheduled · Tomorrow 08:00", "Scheduled", "scheduled"], ["Recital update", "Parent group · 82 recipients", "Sent", "complete"], ["Costume pickup", "Automated · 31 recipients", "Queued", "in-progress"], ["Staff announcement", "Manual · 12 recipients", "Draft", "scheduled"]] },
  { index: "07", tab: "Staff and CRM", title: "Staff and CRM", description: "Keep staff responsibilities and key studio activity visible so the right people can act on the right work at the right time.", bullets: ["Staff roles and responsibility visibility", "Student and parent activity context", "Task ownership and progress tracking", "Centralized notes and activity history"], head: "Staff and CRM", status: "12 staff members", rows: [["Front desk", "8 active tasks · 3 due today", "On track", "complete"], ["Admin team", "14 active tasks · 2 overdue", "Needs review", "in-progress"], ["Instructors", "21 assigned tasks · 6 today", "On track", "complete"], ["Costume team", "9 open orders · 3 returns", "In progress", "in-progress"], ["Studio director", "4 approvals awaiting review", "Review", "scheduled"]] }
];

const steps = [
  { number: 1, title: "Book a demo", text: "We map Sakolee to your studio’s real workflows, not a generic template." },
  { number: 2, title: "We configure everything", text: "Classes, staff, email workflows, and your existing student data — all set for you." },
  { number: 3, title: "Train your team", text: "A live 45-min walkthrough for admin staff. Most studios are confident within 2–3 hours." },
  { number: 4, title: "Go live", text: "Run your studio from one place. Our support team is available when you need it." }
];

const testimonials = [
  { initials: "KL", name: "Karen Lee", role: "Owner, Momentum Dance Studio", delay: "0s", quote: "Finally, someone made it for dance. We can monitor waitlists, and costume approvals are all linked to one profile. Everything about Sakolee just makes sense." },
  { initials: "RT", name: "Rachel Torres", role: "Director, Starlight Academy", delay: ".1s", quote: "Onboarding tasks used to live in someone’s head. Now every new student automatically gets a checklist and the right person gets assigned. Nothing gets missed anymore." },
  { initials: "MP", name: "Maria Patel", role: "Studio Manager, Arabesque Arts", delay: ".2s", quote: "We used to lose track of failed costume orders. Sakolee puts status front and center. Now vendors, follow-up, and return details are visible to the entire team." }
];

const plans = [
  { name: "Starter", price: "$99", period: "/ month", description: "For small studios just getting organized.", cta: "Get started", features: ["Up to 100 students", "Class and enrollment management", "Student profiles and history", "Email support"] },
  { name: "Studio", price: "$199", period: "/ month", description: "The full Sakolee experience for growing studios.", cta: "Start free trial", featured: true, features: ["Up to 500 students", "Everything in Starter", "Automated onboarding projects and tasks", "Recital, costume library, and order management", "Automated email workflows", "Priority support"] },
  { name: "Enterprise", price: "Custom", description: "For multi-location studios with complex needs.", cta: "Contact sales", features: ["Unlimited students", "Multi-location support", "Custom integrations", "Dedicated account manager", "SLA guarantee"] }
];
</script>
