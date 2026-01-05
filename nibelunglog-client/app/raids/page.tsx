import { api } from '@/lib/api';
import PrimaryButton from '@/components/ui/buttons/PrimaryButton';
import SecondaryButton from '@/components/ui/buttons/SecondaryButton';
import Link from 'next/link';

interface RaidsPageProps {
  searchParams: Promise<{ page?: string; raidTypeName?: string }>;
}

export default async function RaidsPage({ searchParams }: RaidsPageProps) {
  const params = await searchParams;
  const page = parseInt(params.page || '1', 10);
  const raidTypeName = params.raidTypeName;

  let raidsData;
  try {
    raidsData = await api.getRaids({
      page,
      pageSize: 25,
      raidTypeName,
    });
  } catch (error) {
    return (
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="mb-12">
          <h1 className="text-5xl font-bold text-white mb-4">Рейды</h1>
        </div>
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
      <div className="mb-12">
        <h1 className="text-5xl font-bold text-white mb-4">Рейды</h1>
        {raidTypeName && (
          <p className="text-gray-400 text-lg">Фильтр: {raidTypeName}</p>
        )}
      </div>

      {raidsData.data.length === 0 ? (
        <div className="text-center py-20">
          <p className="text-gray-400 text-lg">Рейды не найдены</p>
        </div>
      ) : (
        <>
          <div className="bg-gray-900 rounded-2xl shadow-2xl border border-gray-800 overflow-hidden mb-12">
            <table className="min-w-full divide-y divide-gray-800">
              <thead className="bg-gray-950">
                <tr>
                  <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                    Рейд
                  </th>
                  <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                    Дата и время
                  </th>
                  <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                    Время
                  </th>
                  <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                    Лидер
                  </th>
                  <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                    Боссы
                  </th>
                  <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                    Вайпы
                  </th>
                  <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                    Урон
                  </th>
                </tr>
              </thead>
              <tbody className="bg-gray-900 divide-y divide-gray-800">
                {raidsData.data.map((raid) => (
                  <tr key={raid.id} className="hover:bg-gray-800 transition-colors">
                    <td className="px-8 py-5 whitespace-nowrap">
                      <Link href={`/raids/${raid.id}`} className="text-base font-bold text-white hover:text-gray-300">
                        {raid.raidTypeName}
                      </Link>
                    </td>
                    <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-400">
                      {formatDate(raid.startTime)}
                    </td>
                    <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-400">
                      {formatTime(raid.totalTime)}
                    </td>
                    <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-300">
                      {raid.leaderName}
                    </td>
                    <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-300">
                      {raid.completedBosses} / {raid.totalBosses}
                    </td>
                    <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-300">
                      {raid.wipes}
                    </td>
                    <td className="px-8 py-5 whitespace-nowrap text-sm font-bold text-white">
                      {(raid.totalDamage / 1000000).toFixed(1)}M
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="flex justify-center items-center gap-6">
            {page > 1 && (
              <SecondaryButton href={`/raids?page=${page - 1}${raidTypeName ? `&raidTypeName=${encodeURIComponent(raidTypeName)}` : ''}`}>
                Назад
              </SecondaryButton>
            )}
            <span className="text-gray-400 text-lg font-medium">
              Страница {page} из {raidsData.totalPages}
            </span>
            {page < raidsData.totalPages && (
              <PrimaryButton href={`/raids?page=${page + 1}${raidTypeName ? `&raidTypeName=${encodeURIComponent(raidTypeName)}` : ''}`}>
                Вперед
              </PrimaryButton>
            )}
          </div>
        </>
      )}
    </main>
  );
}

