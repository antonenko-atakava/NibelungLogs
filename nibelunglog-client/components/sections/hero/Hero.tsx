import PrimaryButton from '@/components/ui/buttons/PrimaryButton';
import SecondaryButton from '@/components/ui/buttons/SecondaryButton';

export default function Hero() {
  return (
    <section 
      className="relative bg-gradient-to-b from-[var(--background)] via-[var(--background-elevated)] to-[var(--background)] text-[var(--foreground)] py-24 md:py-32 lg:py-40 overflow-hidden"
      aria-labelledby="hero-title"
    >
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_50%,rgba(59,130,246,0.08),transparent)] pointer-events-none"></div>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
        <div className="max-w-4xl">
          <h1 
            id="hero-title"
            className="text-5xl sm:text-6xl md:text-7xl lg:text-8xl font-bold mb-6 md:mb-8 leading-[1.1] tracking-tight"
          >
            Логи рейдов
            <br />
            <span className="bg-gradient-to-r from-[var(--foreground)] via-[var(--foreground-muted)] to-[var(--foreground-subtle)] bg-clip-text text-transparent">
              World of Warcraft
            </span>
          </h1>
          <p className="text-lg sm:text-xl md:text-2xl text-[var(--foreground-muted)] mb-10 md:mb-12 leading-relaxed max-w-2xl">
            Анализ производительности игроков, статистика рейдов и детальная информация о каждом энкаунтере
          </p>
          <div className="flex flex-col sm:flex-row gap-4">
            <PrimaryButton href="/raids" aria-label="Перейти к списку рейдов">
              Просмотреть рейды
            </PrimaryButton>
            <SecondaryButton href="/players" aria-label="Перейти к списку игроков">
              Топ игроков
            </SecondaryButton>
          </div>
        </div>
      </div>
    </section>
  );
}

