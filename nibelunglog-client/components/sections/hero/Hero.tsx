import PrimaryButton from '@/components/ui/buttons/PrimaryButton';
import SecondaryButton from '@/components/ui/buttons/SecondaryButton';

export default function Hero() {
  return (
    <section className="relative bg-gradient-to-b from-black via-gray-950 to-black text-white py-32 overflow-hidden">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_50%,rgba(120,119,198,0.1),transparent)]"></div>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
        <div className="max-w-4xl">
          <h1 className="text-6xl md:text-7xl font-bold mb-8 leading-tight tracking-tight">
            Логи рейдов
            <br />
            <span className="bg-gradient-to-r from-white to-gray-400 bg-clip-text text-transparent">
              World of Warcraft
            </span>
          </h1>
          <p className="text-xl md:text-2xl text-gray-400 mb-12 leading-relaxed max-w-2xl">
            Анализ производительности игроков, статистика рейдов и детальная информация о каждом энкаунтере
          </p>
          <div className="flex flex-col sm:flex-row gap-4">
            <PrimaryButton href="/raids">
              Просмотреть рейды
            </PrimaryButton>
            <SecondaryButton href="/players">
              Топ игроков
            </SecondaryButton>
          </div>
        </div>
      </div>
    </section>
  );
}

