import Link from 'next/link';
import { RaidDto } from '@/types/api/RaidDto';

interface RaidCardProps {
  raid: RaidDto;
}

export default function RaidCard({ raid }: RaidCardProps) {
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
    <Link href={`/raids/${raid.id}`} className="block bg-white rounded-xl shadow-lg border border-gray-100 p-8 hover:shadow-2xl hover:-translate-y-1 transition-all duration-300 group">
      <div className="flex justify-between items-start mb-6">
        <div>
          <h3 className="text-2xl font-bold text-gray-900 mb-2 group-hover:text-gray-700 transition-colors">{raid.raidTypeName}</h3>
          <p className="text-sm font-medium text-gray-500">{raid.guildName}</p>
        </div>
        <div className="text-right">
          <p className="text-sm font-medium text-gray-600">{formatDate(raid.startTime)}</p>
          <p className="text-xs text-gray-400 mt-1">{formatTime(raid.totalTime)}</p>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-6 pt-6 border-t border-gray-100">
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Лидер</p>
          <p className="text-base font-semibold text-gray-900">{raid.leaderName}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Боссы</p>
          <p className="text-base font-semibold text-gray-900">{raid.completedBosses} / {raid.totalBosses}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Вайпы</p>
          <p className="text-base font-semibold text-gray-900">{raid.wipes}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Урон</p>
          <p className="text-base font-semibold text-gray-900">{(raid.totalDamage / 1000000).toFixed(1)}M</p>
        </div>
      </div>
    </Link>
  );
}

