import { api } from '@/lib/api';
import { notFound } from 'next/navigation';
import Link from 'next/link';

interface RaidDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function RaidDetailPage({ params }: RaidDetailPageProps) {
  const { id } = await params;
  const raidId = parseInt(id, 10);

  if (isNaN(raidId))
    notFound();

  let raid;
  try {
    raid = await api.getRaid(raidId);
  } catch (error) {
    return (
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <Link href="/raids" className="text-gray-400 hover:text-white mb-8 inline-block transition-colors">
          ← Назад к рейдам
        </Link>
        <div className="text-center py-20">
          <p className="text-red-400 text-lg mb-4">Ошибка подключения к API</p>
          <p className="text-gray-400 text-sm">Убедитесь, что API сервер запущен на http://localhost:5097</p>
        </div>
      </main>
    );
  }

  const formatTime = (seconds: number) => {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <Link href="/raids" className="text-gray-400 hover:text-white mb-8 inline-block transition-colors">
        ← Назад к рейдам
      </Link>

      <div className="bg-gray-900 rounded-2xl shadow-2xl border border-gray-800 p-10 mb-12">
        <h1 className="text-4xl font-bold text-white mb-8">{raid.raidTypeName}</h1>
        
        <div className="grid grid-cols-2 md:grid-cols-4 gap-8 mb-8">
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Гильдия</p>
            <p className="text-lg font-bold text-white">{raid.guildName}</p>
          </div>
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Лидер</p>
            <p className="text-lg font-bold text-white">{raid.leaderName}</p>
          </div>
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Дата</p>
            <p className="text-lg font-bold text-white">{formatDate(raid.startTime)}</p>
          </div>
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Время</p>
            <p className="text-lg font-bold text-white">{formatTime(raid.totalTime)}</p>
          </div>
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Боссы</p>
            <p className="text-lg font-bold text-white">{raid.completedBosses} / {raid.totalBosses}</p>
          </div>
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Вайпы</p>
            <p className="text-lg font-bold text-white">{raid.wipes}</p>
          </div>
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Урон</p>
            <p className="text-lg font-bold text-white">{(raid.totalDamage / 1000000).toFixed(1)}M</p>
          </div>
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">Лечение</p>
            <p className="text-lg font-bold text-white">{(raid.totalHealing / 1000000).toFixed(1)}M</p>
          </div>
        </div>
      </div>

      <div className="mb-8">
        <h2 className="text-4xl font-bold text-white mb-6">Энкаунтеры</h2>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        {raid.encounters.map((encounter) => (
          <Link
            key={encounter.id}
            href={`/encounters/${encounter.id}`}
            className="block bg-gray-900 rounded-xl shadow-lg border border-gray-800 p-8 hover:shadow-2xl hover:-translate-y-1 transition-all duration-300 group"
          >
            <div className="flex justify-between items-start mb-6">
              <div>
                <h3 className="text-2xl font-bold text-white mb-2 group-hover:text-gray-300 transition-colors">
                  {encounter.encounterName || encounter.encounterEntry}
                </h3>
                <p className={`text-sm font-semibold ${encounter.success ? 'text-green-400' : 'text-red-400'}`}>
                  {encounter.success ? 'Успешно' : 'Провалено'}
                </p>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-6 pt-6 border-t border-gray-800">
              <div>
                <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Урон</p>
                <p className="text-base font-bold text-white">{(encounter.totalDamage / 1000000).toFixed(1)}M</p>
              </div>
              <div>
                <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Лечение</p>
                <p className="text-base font-bold text-white">{(encounter.totalHealing / 1000000).toFixed(1)}M</p>
              </div>
              <div>
                <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Танки</p>
                <p className="text-base font-bold text-white">{encounter.tanks}</p>
              </div>
              <div>
                <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Хилы</p>
                <p className="text-base font-bold text-white">{encounter.healers}</p>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </main>
  );
}

